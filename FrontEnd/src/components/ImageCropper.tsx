import { useState, useRef, PointerEvent } from "react";
import classnames from "classnames";

import { UiLogicError, geometry } from "./common";

import BlobImage from "./BlobImage";

import "./ImageCropper.css";

const { Rect } = geometry;

type Rect = geometry.Rect;
type Movement = geometry.Movement;

export function crop(
  image: HTMLImageElement,
  clip: Rect,
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

interface ImageInfo {
  element: HTMLImageElement;
  width: number;
  height: number;
  ratio: number;
  landscape: boolean;
  rect: Rect;
}

export interface CropConstraint {
  ratio?: number;
  //   minClipWidth?: number;
  //   minClipHeight?: number;
  //   maxClipWidth?: number;
  //   maxClipHeight?: number;
}

function generateImageInfo(imageElement: HTMLImageElement): ImageInfo {
  const { naturalWidth, naturalHeight } = imageElement;
  const imageRatio = naturalHeight / naturalWidth;

  return {
    element: imageElement,
    width: naturalWidth,
    height: naturalHeight,
    ratio: imageRatio,
    landscape: imageRatio < 1,
    rect: new Rect(0, 0, naturalWidth, naturalHeight),
  };
}

interface ImageCropperProps {
  clip: Rect;
  image: Blob | string | null;
  imageElementCallback: (element: HTMLImageElement | null) => void;
  onImageLoad: () => void;
  onMove: (movement: Movement, originalClip: Rect) => void;
  onResize: (movement: Movement, originalClip: Rect) => void;
  containerClassName?: string;
}

export function useImageCrop(
  file: File | null,
  options?: {
    constraint?: CropConstraint;
  },
): {
  clip: Rect;
  setClip: (clip: Rect) => void;
  canCrop: boolean;
  crop: () => Promise<Blob>;
  imageCropperProps: ImageCropperProps;
} {
  const targetRatio = options?.constraint?.ratio;

  const [imageElement, setImageElement] = useState<HTMLImageElement | null>(
    null,
  );
  const [imageInfo, setImageInfo] = useState<ImageInfo | null>(null);
  const [clip, setClip] = useState<Rect>(Rect.empty);

  if (imageElement == null && imageInfo != null) {
    setImageInfo(null);
    setClip(Rect.empty);
  }

  const canCrop = file != null && imageElement != null && imageInfo != null;

  return {
    clip,
    setClip,
    canCrop,
    crop() {
      if (!canCrop) throw new UiLogicError();
      return crop(imageElement, clip, file.type);
    },
    imageCropperProps: {
      clip,
      image: file,
      imageElementCallback: setImageElement,
      onMove: (movement, originalClip) => {
        if (imageInfo == null) return;
        const newClip = geometry.adjustRectToContainer(
          originalClip.copy().move(movement),
          imageInfo.rect,
          "move",
          {
            targetRatio,
          },
        );
        setClip(newClip);
      },
      onResize: (movement, originalClip) => {
        if (imageInfo == null) return;
        const newClip = geometry.adjustRectToContainer(
          originalClip.copy().expand(movement),
          imageInfo.rect,
          "resize",
          { targetRatio, resizeNoFlip: true, ratioCorrectBasedOn: "width" },
        );
        setClip(newClip);
      },
      onImageLoad: () => {
        if (imageElement == null) throw new UiLogicError();
        const image = generateImageInfo(imageElement);
        setImageInfo(image);
        setClip(
          geometry.adjustRectToContainer(Rect.max, image.rect, "both", {
            targetRatio,
          }),
        );
      },
    },
  };
}

interface PointerState {
  x: number;
  y: number;
  pointerId: number;
  originalClip: Rect;
}

const imageCropperHandlerSize = 15;

export function ImageCropper(props: ImageCropperProps) {
  function convertClipToElement(
    clip: Rect,
    imageElement: HTMLImageElement,
  ): Rect {
    const xRatio = imageElement.clientWidth / imageElement.naturalWidth;
    const yRatio = imageElement.clientHeight / imageElement.naturalHeight;
    return Rect.from({
      left: xRatio * clip.left,
      top: yRatio * clip.top,
      width: xRatio * clip.width,
      height: yRatio * clip.height,
    });
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
    imageElementCallback,
    onImageLoad,
    onMove,
    onResize,
    containerClassName,
  } = props;

  const pointerStateRef = useRef<PointerState | null>(null);
  const [imageElement, setImageElement] = useState<HTMLImageElement | null>(
    null,
  );

  const clipInElement: Rect =
    imageElement != null
      ? convertClipToElement(clip, imageElement)
      : Rect.empty;

  const actOnMovement = (
    e: PointerEvent,
    change: (movement: Movement, originalClip: Rect) => void,
  ) => {
    if (
      imageElement == null ||
      pointerStateRef.current == null ||
      pointerStateRef.current.pointerId != e.pointerId
    ) {
      return;
    }

    const { x, y, originalClip } = pointerStateRef.current;

    const movement = {
      x: e.clientX - x,
      y: e.clientY - y,
    };

    change(convertMovementFromElement(movement, imageElement), originalClip);
  };

  const onPointerDown = (e: PointerEvent) => {
    if (imageElement == null || pointerStateRef.current != null) return;

    e.currentTarget.setPointerCapture(e.pointerId);

    pointerStateRef.current = {
      x: e.clientX,
      y: e.clientY,
      pointerId: e.pointerId,
      originalClip: clip,
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
      <BlobImage
        imgRef={(element) => {
          setImageElement(element);
          imageElementCallback(element);
        }}
        src={image}
        onLoad={onImageLoad}
      />
      <div className="cru-image-cropper-mask-container">
        <div
          className="cru-image-cropper-mask"
          style={
            clipInElement == null
              ? undefined
              : {
                  left: clipInElement.left,
                  top: clipInElement.top,
                  width: clipInElement.width,
                  height: clipInElement.height,
                }
          }
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
