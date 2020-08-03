import React from 'react';

import { ExcludeKey } from '../utilities/type';

const BlobImage: React.FC<
  ExcludeKey<React.ImgHTMLAttributes<HTMLImageElement>, 'src'> & { blob?: Blob }
> = (props) => {
  const { blob, ...otherProps } = props;

  const [url, setUrl] = React.useState<string | undefined>(undefined);

  React.useEffect(() => {
    if (blob != null) {
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
