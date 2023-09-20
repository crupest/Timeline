import classNames from "classnames";

import { useC, Text } from "./common";
import { LoadingButton } from "./button";

import "./SearchInput.css";

interface SearchInputProps {
  value: string;
  onChange: (value: string) => void;
  onButtonClick: () => void;
  loading?: boolean;
  className?: string;
  buttonText?: Text;
}

export default function SearchInput({
  value,
  onChange,
  onButtonClick,
  loading,
  className,
  buttonText,
}: SearchInputProps) {
  const c = useC();

  return (
    <div className={classNames("cru-search-input", className)}>
      <input
        type="search"
        className="cru-search-input-input"
        value={value}
        onChange={(event) => {
          const { value } = event.currentTarget;
          onChange(value);
        }}
        onKeyDown={(event) => {
          if (event.key === "Enter") {
            onButtonClick();
            event.preventDefault();
          }
        }}
      />

      <LoadingButton loading={loading} onClick={onButtonClick}>
        {c(buttonText ?? "search")}
      </LoadingButton>
    </div>
  );
}
