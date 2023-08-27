import { ComponentPropsWithoutRef, useState, useEffect, useMemo } from "react";

type BlobImageProps = Omit<ComponentPropsWithoutRef<"img">, "src"> & {
  imgRef?: React.Ref<HTMLImageElement>;
  src?: Blob | string | null;
  keyBySrc?: boolean;
};

export default function BlobImage(props: BlobImageProps) {
  const { imgRef, src, keyBySrc, ...otherProps } = props;

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

  const key = useMemo(() => {
    if (keyBySrc) {
      return url == null ? undefined : btoa(url);
    } else {
      return undefined;
    }
  }, [url, keyBySrc]);

  return <img key={key} ref={imgRef} {...otherProps} src={url ?? undefined} />;
}
