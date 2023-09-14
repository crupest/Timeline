import classNames from "classnames";

import BlobImage from "~/src/components/BlobImage";

interface TimelinePostEditImageProps {
  file: File | null;
  onChange: (file: File | null) => void;
  disabled: boolean;
  className?: string;
}

export default function ImagePostEdit(props: TimelinePostEditImageProps) {
  const { file, onChange, disabled, className } = props;

  return (
    <div
      className={classNames("timeline-edit-image-container", className)}
    >
      <input
        type="file"
        accept="image/*"
        disabled={disabled}
        onChange={(e) => {
          const files = e.target.files;
          if (files == null || files.length === 0) {
            onChange(null);
          } else {
            onChange(files[0]);
          }
        }}
        className="mx-3 my-1"
      />
      {file && <BlobImage src={file} className="timeline-edit-image" />}
    </div>
  );
}
