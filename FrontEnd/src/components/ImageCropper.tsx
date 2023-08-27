import {
  useState,
  useRef,
  SyntheticEvent,
  PointerEvent,
  useMemo,
  MutableRefObject,
} from "react";
import classnames from "classnames";

import { UiLogicError } from "./common";
import BlobImage from "./BlobImage";

import "./ImageCropper.css";

// All in natural size of image.
export interface Clip {
  left: number;
  top: number;
  width: number;
  height: number;
}

export function applyClipToImage(
  image: HTMLImageElement,
  clip: Clip,
  mimeType: string,
): Promise<Blob> {
  return new Promise((resolve, reject) => {
    const canvas = document.createElement("canvas");
    canvas.width = clip.width;
    canvas.height = clip.height;
    const context = canvas.getContext("2d");

    if (context == null) throw new Error("Failed to create context.");

    context.drawImage(
      image,
      clip.left,
      clip.top,
      clip.width,
      clip.height,
      0,
      0,
      clip.width,
      clip.height,
    );

    canvas.toBlob((blob) => {
      if (blob == null) {
        reject(new Error("canvas.toBlob returns null"));
      } else {
        resolve(blob);
      }
    }, mimeType);
  });
}

interface Movement {
  x: number;
  y: number;
}

interface ImageInfo {
  element: HTMLImageElement;
  width: number;
  height: number;
  ratio: number;
  landscape: boolean;
}

export interface CropConstraint {
  ratio?: number;
  //   minClipWidth?: number;
  //   minClipHeight?: number;
  //   maxClipWidth?: number;
  //   maxClipHeight?: number;
}

function generateImageInfo(
  imageElement: HTMLImageElement | null,
): ImageInfo | null {
  if (imageElement == null) return null;

  const { naturalWidth, naturalHeight } = imageElement;
  const imageRatio = naturalHeight / naturalWidth;

  return {
    element: imageElement,
    width: naturalWidth,
    height: naturalHeight,
    ratio: imageRatio,
    landscape: imageRatio < 1,
  };
}

const emptyClip: Clip = {
  left: 0,
  top: 0,
  width: 0,
  height: 0,
};

const allClip : Clip = {
  left: 0,
  top: 0,
  width: Number.MAX_VALUE,
  height: Number.MAX_VALUE,
}

// TODO: Continue here... mode...
function adjustClip(
  clip: Clip,
  mode: "move" | "resize" | "both",
  imageSize: { width: number; height: number },
  targetRatio?: number | null | undefined,
): Clip {
  class ClipGeometry {
    constructor(
      public left: number,
      public top: number,
      public width: number,
      public height: number,
    ) {}

    get right(): number {
      return this.left + this.width;
    }

    set right(value: number) {
      this.width = this.left + value;
    }

    get bottom(): number {
      return this.top + this.height;
    }

    set bottom(value: number) {
      this.height = this.top + value;
    }

    get ratio(): number {
      return this.height / this.width;
    }

    toClip(): Clip {
      return {
        left: this.left,
        top: this.top,
        width: this.width,
        height: this.height,
      };
    }
  }

  const clipGeometry = new ClipGeometry(
    clip.left,
    clip.top,
    clip.width,
    clip.height,
  );

  // Make clip in image.
  clipGeometry.left = Math.max(clipGeometry.left, 0);
  clipGeometry.top = Math.max(clipGeometry.top, 0);
  clipGeometry.right = Math.min(clipGeometry.right, imageSize.width);
  clipGeometry.bottom = Math.min(clipGeometry.bottom, imageSize.height);

  // Make image "positive"
  if (clipGeometry.right < clipGeometry.left) {
    clipGeometry.right = clipGeometry.left;
  }
  if (clipGeometry.bottom < clipGeometry.top) {
    clipGeometry.bottom = clipGeometry.top;
  }

  // Now correct ratio
  const currentRatio = clipGeometry.ratio;
  if (targetRatio != null && targetRatio > 0 && currentRatio !== targetRatio) {
    if (currentRatio < targetRatio) {
      // too wide
      clipGeometry.width = clipGeometry.height / targetRatio;
    } else {
      clipGeometry.height = clipGeometry.width * targetRatio;
    }
  }

  return clipGeometry.toClip();
}

interface ImageCropperProps {
  clip: Clip;
  image: Blob | string | null;
  imageElementRef: MutableRefObject<HTMLImageElement | null>;
  onImageLoad: (event: SyntheticEvent<HTMLImageElement>) => void;
  onMove: (movement: Movement) => void;
  onResize: (movement: Movement) => void;
  containerClassName?: string;
}

export function useImageCrop(
  file: File | null,
  options?: {
    constraint?: CropConstraint;
  },
): {
  clip: Clip;
  setClip: (clip: Clip) => void;
  canCrop: boolean;
  crop: () => Promise<Blob>;
  imageCropperProps: ImageCropperProps;
} {
  const targetRatio = options?.constraint?.ratio;

  const imageElementRef = useRef<HTMLImageElement | null>(null);
  const [image, setImage] = useState<ImageInfo | null>(null);
  const [clip, setClip] = useState<Clip>(emptyClip  );

  if (imageElementRef.current == null && image != null) {
    setImage(null);
    setClip(emptyClip);
  }

  const canCrop = file != null && image != null;

  const adjustedClip = useMemo(() => {
    return image == null ? emptyClip : adjustClip(clip, image, targetRatio);
  }, [clip, image, targetRatio]);

  return {
    clip,
    setClip,
    canCrop,
    crop() {
      if (!canCrop) throw new UiLogicError();
      return applyClipToImage(image.element, adjustedClip, file.type);
    },
    imageCropperProps: {
      clip: adjustedClip,
      image: file,
      imageElementRef: imageElementRef,
      // TODO: Continue here...
      onMove: ,
      onResize: ,
      onImageLoad: () => {
        const image = generateImageInfo(imageElementRef.current);
        setImage(image);
        setClip(adjustClip(allClip, "both", image, targetRatio));
      },
    },
  };
}

interface PointerState {
  x: number;
  y: number;
  pointerId: number;
}

const imageCropperHandlerSize = 15;

export function ImageCropper(props: ImageCropperProps) {
  function convertClipToElement(
    clip: Clip,
    imageElement: HTMLImageElement,
  ): Clip {
    const xRatio = imageElement.clientWidth / imageElement.naturalWidth;
    const yRatio = imageElement.clientHeight / imageElement.naturalHeight;
    return {
      left: xRatio * clip.left,
      top: yRatio * clip.top,
      width: xRatio * clip.width,
      height: yRatio * clip.height,
    };
  }

  function convertMovementFromElement(
    move: Movement,
    imageElement: HTMLImageElement,
  ): Movement {
    const xRatio = imageElement.naturalWidth / imageElement.clientWidth;
    const yRatio = imageElement.naturalHeight / imageElement.clientHeight;
    return {
      x: xRatio * move.x,
      y: yRatio * move.y,
    };
  }

  const {
    clip,
    image,
    imageElementRef,
    onImageLoad,
    onMove,
    onResize,
    containerClassName,
  } = props;

  const pointerStateRef = useRef<PointerState | null>(null);

  const clipInElement =
    imageElementRef.current != null
      ? convertClipToElement(clip, imageElementRef.current)
      : emptyClip;

  const actOnMovement = (
    e: PointerEvent,
    change: (movement: Movement) => void,
  ) => {
    if (
      imageElementRef.current == null ||
      pointerStateRef.current == null ||
      pointerStateRef.current.pointerId != e.pointerId
    ) {
      return;
    }

    const { x, y } = pointerStateRef.current;

    const movement = {
      x: e.clientX - x,
      y: e.clientY - y,
    };

    change(movement);
  };

  const onPointerDown = (e: PointerEvent) => {
    if (imageElementRef.current == null || pointerStateRef.current != null)
      return;

    e.currentTarget.setPointerCapture(e.pointerId);

    pointerStateRef.current = {
      x: e.clientX,
      y: e.clientY,
      pointerId: e.pointerId,
    };
  };

  const onPointerUp = (e: PointerEvent) => {
    if (
      pointerStateRef.current == null ||
      pointerStateRef.current.pointerId != e.pointerId
    ) {
      return;
    }

    e.currentTarget.releasePointerCapture(e.pointerId);
    pointerStateRef.current = null;
  };

  const onMaskPointerMove = (e: PointerEvent) => {
    actOnMovement(e, onMove);
  };

  const onResizeHandlerPointerMove = (e: PointerEvent) => {
    actOnMovement(e, onResize);
  };

  return (
    <div
      className={classnames("cru-image-cropper-container", containerClassName)}
    >
      <BlobImage imgRef={imageElementRef} src={image} onLoad={onImageLoad} />
      <div className="cru-image-cropper-mask-container">
        <div
          className="cru-image-cropper-mask"
          style={{
            left: clipInElement.left,
            top: clipInElement.top,
            width: clipInElement.width,
            height: clipInElement.height,
          }}
          onPointerMove={onMaskPointerMove}
          onPointerDown={onPointerDown}
          onPointerUp={onPointerUp}
        />
      </div>
      <div
        className="cru-image-cropper-handler"
        style={{
          left:
            clipInElement.left + clipInElement.width - imageCropperHandlerSize,
          top:
            clipInElement.top + clipInElement.height - imageCropperHandlerSize,
          width: imageCropperHandlerSize * 2,
          height: imageCropperHandlerSize * 2,
        }}
        onPointerMove={onResizeHandlerPointerMove}
        onPointerDown={onPointerDown}
        onPointerUp={onPointerUp}
      />
    </div>
  );
}
