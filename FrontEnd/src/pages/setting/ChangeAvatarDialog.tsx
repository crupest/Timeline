import { useState, ChangeEvent, ComponentPropsWithoutRef } from "react";

import { useC, Text, UiLogicError } from "@/common";

import { useUser } from "@/services/user";

import { getHttpUserClient } from "@/http/user";

import ImageCropper, {
  Clip,
  applyClipToImage,
} from "@/views/common/ImageCropper";
import BlobImage from "@/views/common/BlobImage";
import ButtonRowV2 from "@/views/common/button/ButtonRowV2";
import Dialog from "@/views/common/dialog/Dialog";
import DialogContainer from "@/views/common/dialog/DialogContainer";

import "./ChangeAvatarDialog.css";

interface ChangeAvatarDialogProps {
  open: boolean;
  onClose: () => void;
}

export default function ChangeAvatarDialog({
  open,
  onClose,
}: ChangeAvatarDialogProps) {
  const c = useC();

  const user = useUser();

  type State =
    | "select"
    | "crop"
    | "process-crop"
    | "preview"
    | "uploading"
    | "success"
    | "error";
  const [state, setState] = useState<State>("select");

  const [file, setFile] = useState<File | null>(null);
  const [clip, setClip] = useState<Clip | null>(null);
  const [cropImgElement, setCropImgElement] = useState<HTMLImageElement | null>(
    null,
  );
  const [resultBlob, setResultBlob] = useState<Blob | null>(null);
  const [message, setMessage] = useState<Text>(
    "settings.dialogChangeAvatar.prompt.select",
  );

  const close = (): void => {
    if (state !== "uploading") {
      onClose();
    }
  };

  const onSelectFile = (e: ChangeEvent<HTMLInputElement>): void => {
    const files = e.target.files;
    if (files == null || files.length === 0) {
      setFile(null);
    } else {
      setFile(files[0]);
    }
  };

  const onCropNext = () => {
    if (
      cropImgElement == null ||
      clip == null ||
      clip.width === 0 ||
      file == null
    ) {
      throw new UiLogicError();
    }

    setState("process-crop");

    void applyClipToImage(cropImgElement, clip, file.type).then((b) => {
      setState("preview");
      setResultBlob(b);
    });
  };

  const onCropPrevious = () => {
    setFile(null);
    setState("select");
  };

  const onPreviewPrevious = () => {
    setState("crop");
  };

  const upload = () => {
    if (resultBlob == null) {
      throw new UiLogicError();
    }

    if (user == null) {
      throw new UiLogicError();
    }

    setState("uploading");
    getHttpUserClient()
      .putAvatar(user.username, resultBlob)
      .then(
        () => {
          setState("success");
        },
        () => {
          setState("error");
          setMessage("operationDialog.error");
        },
      );
  };

  const cancelButton = {
    key: "cancel",
    action: "secondary",
    text: "operationDialog.cancel",
    onClick: close,
  } as const;

  const createPreviousButton = (onClick: () => void) =>
    ({
      key: "previous",
      action: "secondary",
      text: "operationDialog.previousStep",
      onClick,
    }) as const;

  const buttonsMap: Record<
    State,
    ComponentPropsWithoutRef<typeof ButtonRowV2>["buttons"]
  > = {
    select: [
      cancelButton,
      {
        key: "next",
        action: "primary",
        text: "operationDialog.nextStep",
        onClick: () => setState("crop"),
        disabled: file == null,
      },
    ],
    crop: [
      cancelButton,
      createPreviousButton(onCropPrevious),
      {
        key: "next",
        action: "primary",
        text: "operationDialog.nextStep",
        onClick: onCropNext,
        disabled: cropImgElement == null || clip == null || clip.width === 0,
      },
    ],
    "process-crop": [cancelButton, createPreviousButton(onPreviewPrevious)],
    preview: [
      cancelButton,
      createPreviousButton(onPreviewPrevious),
      {
        key: "upload",
        action: "primary",
        text: "settings.dialogChangeAvatar.upload",
        onClick: upload,
      },
    ],
    uploading: [],
    success: [
      {
        key: "ok",
        text: "operationDialog.ok",
        color: "create",
        onClick: close,
      },
    ],
    error: [
      cancelButton,
      {
        key: "retry",
        action: "primary",
        text: "operationDialog.retry",
        onClick: upload,
      },
    ],
  };

  return (
    <Dialog open={open} onClose={close}>
      <DialogContainer
        title="settings.dialogChangeAvatar.title"
        titleColor="primary"
        buttonsV2={buttonsMap[state]}
      >
        {(() => {
          if (state === "select") {
            return (
              <div className="change-avatar-dialog-container">
                <div className="change-avatar-dialog-prompt">
                  {c("settings.dialogChangeAvatar.prompt.select")}
                </div>
                <input
                  className="change-avatar-select-input"
                  type="file"
                  accept="image/*"
                  onChange={onSelectFile}
                />
              </div>
            );
          } else if (state === "crop") {
            if (file == null) {
              throw new UiLogicError();
            }
            return (
              <div className="change-avatar-dialog-container">
                <ImageCropper
                  className="change-avatar-cropper"
                  clip={clip}
                  onChange={setClip}
                  image={file}
                  imageElementCallback={setCropImgElement}
                />
                <div className="change-avatar-dialog-prompt">
                  {c("settings.dialogChangeAvatar.prompt.crop")}
                </div>
              </div>
            );
          } else if (state === "process-crop") {
            return (
              <div className="change-avatar-dialog-container">
                <div className="change-avatar-dialog-prompt">
                  {c("settings.dialogChangeAvatar.prompt.processingCrop")}
                </div>
              </div>
            );
          } else if (state === "preview") {
            return (
              <div className="change-avatar-dialog-container">
                <BlobImage
                  className="change-avatar-preview-image"
                  src={resultBlob}
                  alt={
                    c("settings.dialogChangeAvatar.previewImgAlt") ?? undefined
                  }
                />
                <div className="change-avatar-dialog-prompt">
                  {c("settings.dialogChangeAvatar.prompt.preview")}
                </div>
              </div>
            );
          } else if (state === "uploading") {
            return (
              <div className="change-avatar-dialog-container">
                <BlobImage
                  className="change-avatar-preview-image"
                  src={resultBlob}
                />
                <div className="change-avatar-dialog-prompt">
                  {c("settings.dialogChangeAvatar.prompt.uploading")}
                </div>
              </div>
            );
          } else if (state === "success") {
            return (
              <div className="change-avatar-dialog-container">
                <div className="change-avatar-dialog-prompt success">
                  {c("operationDialog.success")}
                </div>
              </div>
            );
          } else {
            return (
              <div className="change-avatar-dialog-container">
                <BlobImage
                  className="change-avatar-preview-image"
                  src={resultBlob}
                />
                <div className="change-avatar-dialog-prompt error">
                  {c(message)}
                </div>
              </div>
            );
          }
        })()}
      </DialogContainer>
    </Dialog>
  );
}
