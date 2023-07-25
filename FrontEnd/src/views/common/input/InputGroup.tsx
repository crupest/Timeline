/**
 * Some notes for InputGroup:
 * This is one of the most complicated components in this project.
 * Probably because the feature is complex and involved user inputs.
 *
 * I hope it contains following features:
 * - Input features
 *   - Supports a wide range of input types.
 *   - Validator to validate user inputs.
 *   - Can set initial values.
 *   - Dirty, aka, has user touched this input.
 * - Developer friendly
 *   - Easy to use APIs.
 *   - Type check as much as possible.
 * - UI
 *   - Configurable appearance.
 *   - Can display helper and error messages.
 * - Easy to extend, like new input types.
 *
 * So here is some design decisions:
 * Inputs are identified by its _key_.
 * `InputGroup` component takes care of only UI and no logic.
 * `useInputs` hook takes care of logic and generate props for `InputGroup`.
 */

import { useState, Ref } from "react";
import classNames from "classnames";

import { useC, Text, ThemeColor } from "../common";

import "./InputGroup.css";

export interface InputBase {
  key: string;
  label: Text;
  helper?: Text;
  disabled?: boolean;
  error?: Text;
}

export interface TextInput extends InputBase {
  type: "text";
  value: string;
  password?: boolean;
}

export interface BoolInput extends InputBase {
  type: "bool";
  value: boolean;
}

export interface SelectInputOption {
  value: string;
  label: Text;
  icon?: string;
}

export interface SelectInput extends InputBase {
  type: "select";
  value: string;
  options: SelectInputOption[];
}

export type Input = TextInput | BoolInput | SelectInput;

interface InputInitialValueMap {
  text: string;
  bool: boolean;
  select: string;
}

type Dirties<Inputs extends Input[]> = {
  [Index in keyof Inputs]: boolean;
};

type ExtendInputForComponent<I extends Input> = I & {};

type ExtendInputsForComponent<Inputs extends Input[]> = {
  [Index in keyof Inputs]: ExtendInputForComponent<Inputs[Index]>;
};

type InitialValueTransformer<I extends Input> = (
  input: I,
  value: InputInitialValueMap[I["type"]] | null | undefined,
) => InputValueMap[I["type"]];

type InitialValueTransformers = {
  [I in Input as I["type"]]: InitialValueTransformer<I>;
};

const defaultInitialValueTransformer: InitialValueTransformers = {
  text: (input, value) => value ?? "",
  bool: (input, value) => value ?? false,
  select: (input, value) => value ?? input.options[0].value,
};

export type InputErrors = {
  index: number;
  message: Text;
}[];

export interface InputGroupProps<Inputs extends Input[]> {
  color?: ThemeColor;
  containerClassName?: string;
  containerRef?: Ref<HTMLDivElement>;

  inputs: ExtendInputsForComponent<Inputs>;
  onChange: <Index extends number>(
    index: Index,
    value: InputValueMap[Inputs[Index]["type"]],
  ) => void;
}

export type ExtendInputForHook<I extends Input> = I & {
  initialValue?: InputInitialValueMap[I["type"]] | null;
};

export type ExtendInputsForHook<Inputs extends Input[]> = {
  [Index in keyof Inputs]: ExtendInputForHook<Inputs[Index]>;
};

export type Validator<Inputs extends Input[]> = (
  values: { [Index in keyof Inputs]: InputValueMap[Inputs[Index]["type"]] },
  inputs: Inputs,
) => InputErrors;

export function useInputs<Inputs extends Input[]>(
  inputs: ExtendInputsForHook<Inputs>,
  options: {
    validator?: Validator<Inputs>;
    disabled?: boolean;
  },
): {
  inputGroupProps: ExtendInputsForComponent<Inputs>;
  confirm: (values: Values<Inputs>) => void;
} {
  const { validator, disabled } = options;

  const [values, setValues] = useState<Values<Inputs>>(() =>
    inputs.map((input) =>
      defaultInitialValueTransformer[input.type](input, input.initialValue),
    ),
  );
  const [errors, setErrors] = useState<InputErrors>([]);
  const [dirties, setDirties] = useState<Dirties<Inputs>>();

  const componentInputs: ExtendInputForComponent<Input>[] = [];

  for (let i = 0; i < inputs.length; i++) {
    const input = { ...inputs[i] };
    delete input.initialValue; // No use.
    const error = dirties[i]
      ? errors.find((e) => e.index === i)?.message
      : undefined;
    const componentInput: ExtendInputForComponent<Input> = {
      ...input,
      value: values[i],
      disabled,
      error,
    };
    componentInputs.push(componentInput);
  }

  const dirtyAll = () => {
    if (dirties != null) {
      setDirties(new Array(dirties.length).fill(true) as Dirties<Inputs>);
    }
  };

  return {
    inputGroupProps: {},
  };
}

export function InputGroup<Inputs extends Input[]>({
  color,
  inputs,
  onChange,
  containerRef,
  containerClassName,
}: InputGroupProps<ExtendInputsForComponent<Inputs>>) {
  const c = useC();

  return (
    <div
      ref={containerRef}
      className={classNames(
        "cru-input-group",
        `cru-${color ?? "primary"}`,
        containerClassName,
      )}
    >
      {inputs.map((item, index) => {
        const { type, value, label, error, helper, disabled } = item;

        const getContainerClassName = (
          ...additionalClassNames: classNames.ArgumentArray
        ) =>
          classNames(
            `cru-input-container cru-input-${type}`,
            error && "error",
            ...additionalClassNames,
          );

        const changeValue = (value: InputValueMap[keyof InputValueMap]) => {
          // `map` makes every type info lost, so we let ts do _not_ do type check here.
          onChange(index, value as never);
        };

        if (type === "text") {
          const { password } = item;
          return (
            <div
              key={index}
              className={getContainerClassName(password && "password")}
            >
              {label && <label className="cru-input-label">{c(label)}</label>}
              <input
                type={password ? "password" : "text"}
                value={value as string}
                onChange={(event) => {
                  const v = event.target.value;
                  changeValue(v);
                }}
                disabled={disabled}
              />
              {error && <div className="cru-input-error">{c(error)}</div>}
              {helper && <div className="cru-input-helper">{c(helper)}</div>}
            </div>
          );
        } else if (type === "bool") {
          return (
            <div key={index} className={getContainerClassName()}>
              <input
                type="checkbox"
                checked={value as boolean}
                onChange={(event) => {
                  const v = event.currentTarget.checked;
                  changeValue(v);
                }}
                disabled={disabled}
              />
              <label className="cru-input-label-inline">{c(label)}</label>
              {error && <div className="cru-input-error">{c(error)}</div>}
              {helper && <div className="cru-input-helper">{c(helper)}</div>}
            </div>
          );
        } else if (type === "select") {
          return (
            <div key={index} className={getContainerClassName()}>
              <label className="cru-input-label">{c(label)}</label>
              <select
                value={value as string}
                onChange={(event) => {
                  const e = event.target.value;
                  changeValue(e);
                }}
                disabled={disabled}
              >
                {item.options.map((option, i) => {
                  return (
                    <option value={option.value} key={i}>
                      {option.icon}
                      {c(option.label)}
                    </option>
                  );
                })}
              </select>
            </div>
          );
        }
      })}
    </div>
  );
}
