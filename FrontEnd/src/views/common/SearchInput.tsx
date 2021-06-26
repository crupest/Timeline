import React, { useCallback } from "react";
import classnames from "classnames";
import { useTranslation } from "react-i18next";
import { Spinner, Form, Button } from "react-bootstrap";

export interface SearchInputProps {
  value: string;
  onChange: (value: string) => void;
  onButtonClick: () => void;
  className?: string;
  loading?: boolean;
  buttonText?: string;
  placeholder?: string;
  additionalButton?: React.ReactNode;
  alwaysOneline?: boolean;
}

const SearchInput: React.FC<SearchInputProps> = (props) => {
  const { onChange, onButtonClick, alwaysOneline } = props;

  const { t } = useTranslation();

  const onInputChange = useCallback(
    (event: React.ChangeEvent<HTMLInputElement>): void => {
      onChange(event.currentTarget.value);
    },
    [onChange]
  );

  const onInputKeyPress = useCallback(
    (event: React.KeyboardEvent<HTMLInputElement>): void => {
      if (event.key === "Enter") {
        onButtonClick();
        event.preventDefault();
      }
    },
    [onButtonClick]
  );

  return (
    <div
      className={classnames(
        "cru-search-input",
        alwaysOneline ? "flex-nowrap" : "flex-sm-nowrap",
        props.className
      )}
    >
      <input
        type="text"
        className="me-sm-2 flex-grow-1"
        value={props.value}
        onChange={onInputChange}
        onKeyPress={onInputKeyPress}
        placeholder={props.placeholder}
      />
      {props.additionalButton ? (
        <div className="mt-2 mt-sm-0 flex-shrink-0 order-sm-last ms-sm-2">
          {props.additionalButton}
        </div>
      ) : null}
      <div
        className={classnames(
          alwaysOneline ? "mt-0 ms-2" : "mt-2 mt-sm-0 ms-auto ms-sm-0",
          "flex-shrink-0"
        )}
      >
        {props.loading ? (
          <Spinner variant="primary" animation="border" />
        ) : (
          <Button variant="outline-primary" onClick={props.onButtonClick}>
            {props.buttonText ?? t("search")}
          </Button>
        )}
      </div>
    </div>
  );
};

export default SearchInput;
