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

import { useState, Ref, useId } from "react";
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
// use never so you don't have to cast everywhere
export type InputConfirmValueDict = Record<string, never>;

export type GeneralInputErrorDict =
  | {
      [key: string]: Text | null | undefined;
    }
  | null
  | undefined;

type MakeInputInfo<I extends Input> = Omit<I, "value" | "error" | "disabled">;

export type InputInfo = {
  [I in Input as I["type"]]: MakeInputInfo<I>;
}[Input["type"]];

export type Validator = (
  values: InputValueDict,
  inputs: InputInfo[],
) => GeneralInputErrorDict;

export type InputScheme = {
  inputs: InputInfo[];
  validator?: Validator;
};

export type InputData = {
  values: InputValueDict;
  errors: InputErrorDict;
  disabled: InputDisabledDict;
  dirties: InputDirtyDict;
};

export type State = {
  scheme: InputScheme;
  data: InputData;
};

export type DataInitialization = {
  values?: InputValueDict;
  errors?: GeneralInputErrorDict;
  disabled?: InputDisabledDict;
  dirties?: InputDirtyDict;
};

export type Initialization = {
  scheme: InputScheme;
  dataInit?: DataInitialization;
};

export type GeneralInitialization = Initialization | InputScheme | InputInfo[];

export type Initializer = GeneralInitialization | (() => GeneralInitialization);

export interface InputGroupProps {
  color?: ThemeColor;
  containerClassName?: string;
  containerRef?: Ref<HTMLDivElement>;

  inputs: Input[];
  onChange: (index: number, value: Input["value"]) => void;
}

function cleanObject<V>(o: Record<string, V>): Record<string, NonNullable<V>> {
  const result = { ...o };
  for (const key of Object.keys(result)) {
    if (result[key] == null) {
      delete result[key];
    }
  }
  return result as never;
}

export type ConfirmResult =
  | {
      type: "ok";
      values: InputConfirmValueDict;
    }
  | {
      type: "error";
      errors: InputErrorDict;
    };

function validate(
  validator: Validator | null | undefined,
  values: InputValueDict,
  inputs: InputInfo[],
): InputErrorDict {
  return cleanObject(validator?.(values, inputs) ?? {});
}

export function useInputs(options: { init: Initializer }): {
  inputGroupProps: InputGroupProps;
  hasError: boolean;
  hasErrorAndDirty: boolean;
  confirm: () => ConfirmResult;
  setAllDisabled: (disabled: boolean) => void;
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

  function initialize(generalInitialization: GeneralInitialization): State {
    const initialization: Initialization = Array.isArray(generalInitialization)
      ? { scheme: { inputs: generalInitialization } }
      : "scheme" in generalInitialization
      ? generalInitialization
      : { scheme: generalInitialization };

    const { scheme, dataInit } = initialization;
    const { inputs, validator } = scheme;
    const keys = inputs.map((input) => input.key);

    if (process.env.NODE_ENV === "development") {
      const checkKeys = (dict: Record<string, unknown> | undefined) => {
        if (dict != null) {
          for (const key of Object.keys(dict)) {
            if (!keys.includes(key)) {
              console.warn("");
            }
          }
        }
      };

      checkKeys(dataInit?.values);
      checkKeys(dataInit?.errors ?? {});
      checkKeys(dataInit?.disabled);
      checkKeys(dataInit?.dirties);
    }

    function clean<V>(
      dict: Record<string, V> | null | undefined,
    ): Record<string, NonNullable<V>> {
      return dict != null ? cleanObject(dict) : {};
    }

    const values: InputValueDict = {};
    const disabled: InputDisabledDict = clean(dataInit?.disabled);
    const dirties: InputDirtyDict = clean(dataInit?.dirties);
    const isErrorSet = dataInit?.errors != null;
    let errors: InputErrorDict = clean(dataInit?.errors);

    for (let i = 0; i < inputs.length; i++) {
      const input = inputs[i];
      const { key } = input;

      values[key] = initializeValue(input, dataInit?.values?.[key]);
    }

    if (isErrorSet) {
      if (process.env.NODE_ENV === "development") {
        console.log(
          "You explicitly set errors (not undefined) in initializer, so validator won't run.",
        );
      }
    } else {
      errors = validate(validator, values, inputs);
    }

    return {
      scheme,
      data: {
        values,
        errors,
        disabled,
        dirties,
      },
    };
  }

  const { init } = options;
  const initializer = typeof init === "function" ? init : () => init;

  const [state, setState] = useState<State>(() => initialize(initializer()));

  const { scheme, data } = state;
  const { validator } = scheme;

  function createAllBooleanDict(value: boolean): Record<string, boolean> {
    const result: InputDirtyDict = {};
    for (const key of scheme.inputs.map((input) => input.key)) {
      result[key] = value;
    }
    return result;
  }

  const createAllDirties = () => createAllBooleanDict(true);

  const componentInputs: Input[] = [];

  for (let i = 0; i < scheme.inputs.length; i++) {
    const input = scheme.inputs[i];
    const value = data.values[input.key];
    const error = data.errors[input.key];
    const disabled = data.disabled[input.key] ?? false;
    const dirty = data.dirties[input.key] ?? false;
    const componentInput: Input = {
      ...input,
      value: value as never,
      disabled,
      error: dirty ? error : undefined,
    };
    componentInputs.push(componentInput);
  }

  const hasError = Object.keys(data.errors).length > 0;
  const hasDirty = Object.keys(data.dirties).some((key) => data.dirties[key]);

  return {
    inputGroupProps: {
      inputs: componentInputs,
      onChange: (index, value) => {
        const input = scheme.inputs[index];
        const { key } = input;
        const newValues = { ...data.values, [key]: value };
        const newDirties = { ...data.dirties, [key]: true };
        const newErrors = validate(validator, newValues, scheme.inputs);
        setState({
          scheme,
          data: {
            ...data,
            values: newValues,
            errors: newErrors,
            dirties: newDirties,
          },
        });
      },
    },
    hasError,
    hasErrorAndDirty: hasError && hasDirty,
    confirm() {
      const newDirties = createAllDirties();
      const newErrors = validate(validator, data.values, scheme.inputs);

      setState({
        scheme,
        data: {
          ...data,
          dirties: newDirties,
          errors: newErrors,
        },
      });

      if (Object.keys(newErrors).length !== 0) {
        return {
          type: "error",
          errors: newErrors,
        };
      } else {
        return {
          type: "ok",
          values: data.values as InputConfirmValueDict,
        };
      }
    },
    setAllDisabled(disabled: boolean) {
      setState({
        scheme,
        data: {
          ...data,
          disabled: createAllBooleanDict(disabled),
        },
      });
    },
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

  const id = useId();

  return (
    <div
      ref={containerRef}
      className={classNames(
        "cru-input-group",
        `cru-clickable-${color ?? "primary"}`,
        containerClassName,
      )}
    >
      {inputs.map((item, index) => {
        const { key, type, value, label, error, helper, disabled } = item;

        const getContainerClassName = (
          ...additionalClassNames: classNames.ArgumentArray
        ) =>
          classNames(
            `cru-input-container cru-input-type-${type}`,
            error && "error",
            ...additionalClassNames,
          );

        const changeValue = (value: InputValue) => {
          onChange(index, value);
        };

        const inputId = `${id}-${key}`;

        if (type === "text") {
          const { password } = item;
          return (
            <div
              key={key}
              className={getContainerClassName(password && "password")}
            >
              {label && (
                <label className="cru-input-label" htmlFor={inputId}>
                  {c(label)}
                </label>
              )}
              <input
                id={inputId}
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
            <div key={key} className={getContainerClassName()}>
              <input
                id={inputId}
                type="checkbox"
                checked={value}
                onChange={(event) => {
                  const v = event.currentTarget.checked;
                  changeValue(v);
                }}
                disabled={disabled}
              />
              <label className="cru-input-label-inline" htmlFor={inputId}>
                {c(label)}
              </label>
              {error && <div className="cru-input-error">{c(error)}</div>}
              {helper && <div className="cru-input-helper">{c(helper)}</div>}
            </div>
          );
        } else if (type === "select") {
          return (
            <div key={key} className={getContainerClassName()}>
              <label className="cru-input-label" htmlFor={inputId}>
                {c(label)}
              </label>
              <select
                id={inputId}
                value={value}
                onChange={(event) => {
                  const e = event.target.value;
                  changeValue(e);
                }}
                disabled={disabled}
              >
                {item.options.map((option) => {
                  return (
                    <option value={option.value} key={option.value}>
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
