import {
  useState,
  useEffect,
  ReactNode,
  ComponentPropsWithoutRef,
  Ref,
} from "react";
import classNames from "classnames";

import { useC, Text, ThemeColor } from "../common";

import "./InputGroup.css";

export interface TextInput {
  type: "text";
  label?: Text;
  password?: boolean;
  textFieldProps?: Omit<
    ComponentPropsWithoutRef<"input">,
    "type" | "value" | "onChange"
  >;
  helperText?: Text;
}

export interface BoolInput {
  type: "bool";
  label: Text;
  helperText?: Text;
}

export interface SelectInputOption {
  value: string;
  label: Text;
  icon?: ReactNode;
}

export interface SelectInput {
  type: "select";
  label: Text;
  options: SelectInputOption[];
}

export interface DateTimeInput {
  type: "datetime";
  label?: Text;
  helperText?: string;
}

export type Input = TextInput | BoolInput | SelectInput | DateTimeInput;

export type InputType = Input["type"];

export type InputTypeToInputMap = {
  [I in Input as I["type"]]: I;
};

export interface InputTypeToValueMap {
  text: string;
  bool: boolean;
  select: string;
  datetime: Date;
}

export type InputValue = InputTypeToValueMap[keyof InputTypeToValueMap];

export type MapInputToValue<I extends Input> = InputTypeToValueMap[I["type"]];

export type MapInputListToValueList<Tuple extends Input[]> = {
  [Index in keyof Tuple]: MapInputToValue<Tuple[Index]>;
};

export type MapInputListTo<Tuple extends Input[], T> = {
  [Index in keyof Tuple]: T;
};

export interface InputTypeToInitialValueMap {
  text: string | null | undefined;
  bool: boolean | null | undefined;
  select: string | null | undefined;
  datetime: Date | string | null | undefined;
}

export type MapInputToInitialValue<I extends Input> =
  InputTypeToInitialValueMap[I["type"]];

export type InputValueTransformers = {
  [I in Input as I["type"]]: (
    input: I,
    value: MapInputToInitialValue<I>,
  ) => MapInputToValue<I>;
};

const initialValueTransformers: InputValueTransformers = {
  text: (input, value) => value ?? "",
  bool: (input, value) => value ?? false,
  select: (input, value) => value ?? input.options[0].value,
  datetime: (input, value) => {
    if (value == null) return new Date();
    if (typeof value === "string") {
      return new Date(value);
    }
    return value;
  },
};

// No use currently
//
// export type ValueValueTransformers = {
//   [I in Input as I["type"]]: (input: MapInputToValue<I>) => MapInputToValue<I>;
// };
//
// const finalValueMapperMap: ValueValueMapper = {
//   bool: (value) => value,
//   datetime: (value) => new Date(value).toISOString(),
//   select: (value) => value,
//   text: (value) => value,
// };

export type InputErrors = {
  index: number;
  message: Text;
}[];

export type InputList = Input[];
export type Validator<Inputs extends InputList> = (
  inputs: MapInputListToValueList<Inputs>,
) => InputErrors;
export type Values<Inputs extends InputList> = MapInputListToValueList<Inputs>;
export type Dirties<Inputs extends InputList> = MapInputListTo<Inputs, boolean>;

export function useInputs<Inputs extends InputList>(
  inputs: Inputs,
  validator?: Validator<Inputs>,
): {
  inputs: Inputs;
  validator?: Validator<Inputs>;
  dirties: Dirties<Inputs> | undefined;
  setDirties: (dirties: Dirties<Inputs>) => void;
  dirtyAll: () => void;
} {
  const [dirties, setDirties] = useState<Dirties<Inputs>>();

  return {
    inputs,
    validator,
    values,
    dirties,
    setDirties,
    dirtyAll: () => {
      if (dirties != null) {
        setDirties(new Array(dirties.length).fill(true) as Dirties<Inputs>);
      }
    },
  };
}

export interface InputGroupProps<Inputs extends InputList> {
  inputs: Inputs;
  validator?: Validator<Inputs>;

  values?: Values<Inputs>;
  onChange: (
    values: Values<Inputs>,
    errors: InputErrors,
    trigger: number, // May be -1, which means I don't know who trigger this change.
  ) => void;
  errors: InputErrors;
  dirties: Dirties<Inputs>;
  onDirty: (dirties: Dirties<Inputs>) => void;
  disabled?: boolean;

  color?: ThemeColor;
  className?: string;
  containerRef?: Ref<HTMLDivElement>;
}

export default function InputGroup<Inputs extends Input[]>({
  color,
  inputs,
  validator,
  values,
  errors,
  disabled,
  onChange,
  dirties,
  onDirty,
  containerRef,
  className,
}: InputGroupProps<Inputs>) {
  const c = useC();

  type Values = MapInputListToValueList<Inputs>;
  type Dirties = MapInputListTo<Inputs, boolean>;

  useEffect(() => {
    if (values == null) {
      const values = inputs.map((input) => {
        return initialValueTransformers[input.type](input as never);
      }) as Values;
      const errors = validator?.(values) ?? [];
      onChange(values, errors, -1);
      onDirty?.(inputs.map(() => false) as Dirties);
    }
  }, [values, inputs, validator, onChange, onDirty]);

  if (values == null) {
    return null;
  }

  const updateValue = (index: number, newValue: InputValue): void => {
    const oldValues = values;
    const newValues = oldValues.slice() as Values;
    newValues[index] = newValue;
    const error = validator?.(newValues) ?? [];
    onChange(newValues, error, index);
    if (dirties != null && onDirty != null && dirties[index] === false) {
      const newDirties = dirties.slice() as Dirties;
      newDirties[index] = true;
      onDirty(newDirties);
    }
  };

  return (
    <div
      ref={containerRef}
      className={classNames(
        "cru-input-group",
        `cru-${color ?? "primary"}`,
        className,
      )}
    >
      {inputs.map((item: Input, index: number) => {
        const value = values[index];
        const error =
          dirties &&
          dirties[index] &&
          errors &&
          errors.find((e) => e.index === index)?.message;

        if (item.type === "text") {
          return (
            <div
              key={index}
              className={classNames(
                "cru-input-container cru-input-text",
                item.password && "password",
                error && "error",
              )}
            >
              {item.label && (
                <label className="cru-input-label">{c(item.label)}</label>
              )}
              <input
                type={item.password === true ? "password" : "text"}
                value={value as string}
                onChange={(event) => {
                  const v = event.target.value;
                  updateValue(index, v);
                }}
                disabled={disabled}
              />
              {error && <div className="cru-input-error-text">{c(error)}</div>}
              {item.helperText && (
                <div className="cru-input-helper-text">
                  {c(item.helperText)}
                </div>
              )}
            </div>
          );
        } else if (item.type === "bool") {
          return (
            <div
              key={index}
              className={classNames(
                "cru-input-container cru-input-bool",
                error && "error",
              )}
            >
              <input
                type="checkbox"
                checked={value as boolean}
                onChange={(event) => {
                  const v = event.currentTarget.checked;
                  updateValue(index, v);
                }}
                disabled={disabled}
              />
              <label className="cru-input-label-inline">{c(item.label)}</label>
              {error && <div className="cru-input-error-text">{c(error)}</div>}
              {item.helperText && (
                <div className="cru-operation-dialog-helper-text">
                  {c(item.helperText)}
                </div>
              )}
            </div>
          );
        } else if (item.type === "select") {
          return (
            <div
              key={index}
              className={classNames(
                "cru-input-container cru-input-select",
                error && "error",
              )}
            >
              <label className="cru-input-label">{c(item.label)}</label>
              <select
                value={value as string}
                onChange={(event) => {
                  const e = event.target.value;
                  updateValue(index, e);
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
        } else if (item.type === "datetime") {
          return (
            <div
              key={index}
              className={classNames(
                "cru-input-container cru-input-datetime",
                error && "error",
              )}
            >
              {item.label && (
                <label className="cru-input-label">{c(item.label)}</label>
              )}
              <input
                type="datetime-local"
                value={(value as Date).toLocaleString()}
                onChange={(event) => {
                  const v = event.target.valueAsDate;
                  if (v == null) {
                    if (process.env.NODE_ENV === "development") {
                      console.log(
                        "Looks like user input date is null. We do nothing. But you might want to check why.",
                      );
                    }
                    return;
                  }
                  updateValue(index, v);
                }}
                disabled={disabled}
              />
              {error && <div className="cru-input-error-text">{c(error)}</div>}
            </div>
          );
        }
      })}
    </div>
  );
}
