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

import { useState, useRef, Ref } from "react";
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

export type InputValue = Input["value"];

export type InputValueDict = Record<string, InputValue>;
export type InputErrorDict = Record<string, Text>;
export type InputDisabledDict = Record<string, boolean>;
export type InputDirtyDict = Record<string, boolean>;

type MakeInputInfo<I extends Input> = Omit<I, "value" | "error" | "disabled">;

export type InputInfo = {
  [I in Input as I["type"]]: MakeInputInfo<I>;
}[Input["type"]];

export type Validator = (
  values: InputValueDict,
  inputs: InputInfo[],
) => InputErrorDict;

export type InputScheme = {
  inputs: InputInfo[];
  validator?: Validator;
};

export type InputState = {
  values: InputValueDict;
  errors: InputErrorDict;
  disabled: InputDisabledDict;
  dirties: InputDirtyDict;
};

export type State = {
  scheme: InputScheme;
  state: InputState;
};

export type StateInitializer = Partial<InputState>;

export type Initializer = {
  scheme: InputScheme;
  stateInit?: Partial<InputState>;
};

export interface InputGroupProps {
  color?: ThemeColor;
  containerClassName?: string;
  containerRef?: Ref<HTMLDivElement>;

  inputs: Input[];
  onChange: (index: number, value: Input["value"]) => void;
}

function cleanObject<O extends Record<string, unknown>>(o: O): O {
  const result = { ...o };
  for (const key of Object.keys(result)) {
    if (result[key] == null) {
      delete result[key];
    }
  }
  return result;
}

export function useInputs(options: { init?: () => Initializer }): {
  inputGroupProps: InputGroupProps;
} {
  function initializeValue(
    input: InputInfo,
    value?: InputValue | null,
  ): InputValue {
    if (input.type === "text") {
      return value ?? "";
    } else if (input.type === "bool") {
      return value ?? false;
    } else if (input.type === "select") {
      return value ?? input.options[0].value;
    }
    throw new Error("Unknown input type");
  }

  function initialize(initializer: Initializer): State {
    const { scheme, stateInit } = initializer;
    const { inputs, validator } = scheme;
    const keys = inputs.map((input) => input.key);

    if (process.env.NODE_ENV === "development") {
      const checkKeys = (dict: Record<string, unknown>) => {
        for (const key of Object.keys(dict)) {
          if (!keys.includes(key)) {
            console.warn("");
          }
        }
      };

      checkKeys(stateInit?.values ?? {});
      checkKeys(stateInit?.errors ?? {});
      checkKeys(stateInit?.disabled ?? {});
      checkKeys(stateInit?.dirties ?? {});
    }

    const values: InputValueDict = {};
    let errors: InputErrorDict = cleanObject(
      initializer.stateInit?.errors ?? {},
    );
    const disabled: InputDisabledDict = cleanObject(
      initializer.stateInit?.disabled ?? {},
    );
    const dirties: InputDirtyDict = cleanObject(
      initializer.stateInit?.dirties ?? {},
    );

    for (let i = 0; i < inputs.length; i++) {
      const input = inputs[i];
      const { key } = input;
      values[key] = initializeValue(input, stateInit?.values?.[key]);

      if (!(key in dirties)) {
        dirties[key] = false;
      }
    }

    if (Object.keys(errors).length === 0 && validator != null) {
      errors = validator(values, inputs);
    }

    return {
      scheme,
      state: {
        values,
        errors,
        disabled,
        dirties,
      },
    };
  }

  const { init } = options;

  const componentInputs: Input[] = [];

  for (let i = 0; i < inputs.length; i++) {
    const input = { ...inputs[i] };
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

export function InputGroup({
  color,
  inputs,
  onChange,
  containerRef,
  containerClassName,
}: InputGroupProps) {
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

        const changeValue = (value: InputValue) => {
          onChange(index, value);
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
                value={value}
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
                checked={value}
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
                value={value}
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
