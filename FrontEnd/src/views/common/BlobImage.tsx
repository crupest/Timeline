import * as React from "react";

const BlobImage: React.FC<
  Omit<React.ImgHTMLAttributes<HTMLImageElement>, "src"> & {
    blob?: Blob | unknown;
  }
> = (props) => {
  const { blob, ...otherProps } = props;

  const [url, setUrl] = React.useState<string | undefined>(undefined);

  React.useEffect(() => {
    if (blob instanceof Blob) {
      const url = URL.createObjectURL(blob);
      setUrl(url);
      return () => {
        URL.revokeObjectURL(url);
      };
    } else {
      setUrl(undefined);
    }
  }, [blob]);

  return <img {...otherProps} src={url} />;
};

export default BlobImage;
