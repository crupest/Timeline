import classNames from "classnames";

import "./PlainTextPostEdit.css";

interface TimelinePostEditTextProps {
  text: string;
  disabled: boolean;
  onChange: (text: string) => void;
  className?: string;
}

export default function TimelinePostEditText(props: TimelinePostEditTextProps) {
  const { text, disabled, onChange, className } = props;

  return (
    <div
      className={classNames("timeline-edit-plain-text-container", className)}
    >
      <textarea
        value={text}
        disabled={disabled}
        onChange={(event) => {
          onChange(event.target.value);
        }}
        className={classNames("timeline-edit-plain-text-input")}
      />
    </div>
  );
}
