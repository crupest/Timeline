import { ComponentPropsWithoutRef, useState, useEffect } from "react";

type BlobImageProps = Omit<ComponentPropsWithoutRef<"img">, "src"> & {
  imgRef?: React.Ref<HTMLImageElement>;
  src?: Blob | string | null;
};

export default function BlobImage(props: BlobImageProps) {
  const { imgRef, src, ...otherProps } = props;

  const [url, setUrl] = useState<string | null | undefined>(undefined);

  useEffect(() => {
    if (src instanceof Blob) {
      const url = URL.createObjectURL(src);
      setUrl(url);
      return () => {
        URL.revokeObjectURL(url);
      };
    } else {
      setUrl(src);
    }
  }, [src]);

  return <img ref={imgRef} {...otherProps} src={url ?? undefined} />;
}
