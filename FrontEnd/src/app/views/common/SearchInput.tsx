import React, { useCallback } from "react";
import clsx from "clsx";
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
}

const SearchInput: React.FC<SearchInputProps> = (props) => {
  const { onChange, onButtonClick } = props;

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
      }
    },
    [onButtonClick]
  );

  return (
    <Form inline className={clsx("my-2", props.className)}>
      <Form.Control
        className="mr-sm-2 flex-grow-1"
        value={props.value}
        onChange={onInputChange}
        onKeyPress={onInputKeyPress}
        placeholder={props.placeholder}
      />
      <div className="mt-2 mt-sm-0 order-sm-last ml-sm-3">
        {props.additionalButton}
      </div>
      <div className="mt-2 mt-sm-0 ml-auto ml-sm-0">
        {props.loading ? (
          <Spinner variant="primary" animation="border" />
        ) : (
          <Button variant="outline-primary" onClick={props.onButtonClick}>
            {props.buttonText ?? t("search")}
          </Button>
        )}
      </div>
    </Form>
  );
};

export default SearchInput;
