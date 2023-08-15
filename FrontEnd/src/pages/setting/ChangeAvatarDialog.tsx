import {
  useState,
  useEffect,
  ChangeEvent,
  ComponentPropsWithoutRef,
} from "react";
import { AxiosError } from "axios";

import { useC, Text, UiLogicError } from "@/common";

import { useUser } from "@/services/user";

import { getHttpUserClient } from "@/http/user";

import ImageCropper, {
  Clip,
  applyClipToImage,
} from "@/views/common/ImageCropper";
import Button from "@/views/common/button/Button";
import ButtonRow from "@/views/common/button/ButtonRow";
import Dialog from "@/views/common/dialog/Dialog";
import DialogContainer from "@/views/common/dialog/DialogContainer";

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

  const [file, setFile] = useState<File | null>(null);
  const [fileUrl, setFileUrl] = useState<string | null>(null);
  const [clip, setClip] = useState<Clip | null>(null);
  const [cropImgElement, setCropImgElement] = useState<HTMLImageElement | null>(
    null,
  );
  const [resultBlob, setResultBlob] = useState<Blob | null>(null);
  const [resultUrl, setResultUrl] = useState<string | null>(null);

  type State =
    | "select"
    | "crop"
    | "process-crop"
    | "preview"
    | "uploading"
    | "success"
    | "error";
  const [state, setState] = useState<State>("select");

  const [message, setMessage] = useState<Text>(
    "settings.dialogChangeAvatar.prompt.select",
  );

  const trueMessage = c(message);

  const close = (): void => {
    if (!(state === "uploading")) {
      onClose();
    }
  };

  useEffect(() => {
    if (file != null) {
      const url = URL.createObjectURL(file);
      setClip(null);
      setFileUrl(url);
      setState("crop");
      return () => {
        URL.revokeObjectURL(url);
      };
    } else {
      setFileUrl(null);
      setState("select");
    }
  }, [file]);

  useEffect(() => {
    if (resultBlob != null) {
      const url = URL.createObjectURL(resultBlob);
      setResultUrl(url);
      setState("preview");
      return () => {
        URL.revokeObjectURL(url);
      };
    } else {
      setResultUrl(null);
    }
  }, [resultBlob]);

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
      setResultBlob(b);
    });
  };

  const onCropPrevious = () => {
    setFile(null);
    setState("select");
  };

  const onPreviewPrevious = () => {
    setResultBlob(null);
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
        (e: unknown) => {
          setState("error");
          setMessage({ type: "custom", value: (e as AxiosError).message });
        },
      );
  };

  const createPreviewRow = (): React.ReactElement => {
    if (resultUrl == null) {
      throw new UiLogicError();
    }
    return (
      <div className="row justify-content-center">
        <div className="col col-auto">
          <img
            className="change-avatar-img"
            src={resultUrl}
            alt={c("settings.dialogChangeAvatar.previewImgAlt") ?? undefined}
            alt={c("settings.dialogChangeAvatar.previewImgAlt") ?? undefined}
          />
        </div>
      </div>
    );
  };

  const buttonsMap: Record<
    State,
    ComponentPropsWithoutRef<typeof ButtonRow>["buttons"]
  > = {
    select: [
      {
        key: "cancel",
        type: "normal",
        props: {
          outline: true,
          color: "secondary",
          text: "operationDialog.cancel",
          onClick: close,
        },
      },
    ],
    crop: [
      {
        key: "cancel",
        type: "normal",
        props: {
          outline: true,
          color: "secondary",
          text: "operationDialog.cancel",
          onClick: close,
        },
      },
      {
        key: "previous",
        type: "normal",
        props: {
          outline: true,
          color: "secondary",
          text: "operationDialog.previousStep",
          onClick: onCropPrevious,
        },
      },
      {
        key: "next",
        type: "normal",
        props: {
          color: "primary",
          text: "operationDialog.nextStep",
          onClick: onCropNext,
          disabled: cropImgElement == null || clip == null || clip.width === 0,
        },
      },
    ],
  };

  return (
    <Dialog open={open} onClose={close}>
      <DialogContainer
        title="settings.dialogChangeAvatar.title"
        titleColor="primary"
        buttons={buttonsMap[state]}
      >
        {(() => {
          if (state === "select") {
            return (
              <div className="">
                <div className="row">
                  {c("settings.dialogChangeAvatar.prompt.select")}
                </div>
                <div className="row">
                  <input
                    className="px-0"
                    type="file"
                    accept="image/*"
                    onChange={onSelectFile}
                  />
                </div>
              </div>
            );
          } else if (state === "crop") {
            if (fileUrl == null) {
              throw new UiLogicError();
            }
            return (
              <div className="container">
                <div className="row justify-content-center">
                  <ImageCropper
                    clip={clip}
                    onChange={setClip}
                    imageUrl={fileUrl}
                    imageElementCallback={setCropImgElement}
                  />
                </div>
                <div className="row">
                  {c("settings.dialogChangeAvatar.prompt.crop")}
                </div>
              </div>
            );
          } else if (state === "process-crop") {
            return (
              <>
                <div className="container">
                  <div className="row">
                    {c("settings.dialogChangeAvatar.prompt.processingCrop")}
                  </div>
                </div>
                <hr />
                <div className="cru-dialog-bottom-area">
                  <Button
                    text="operationDialog.cancel"
                    color="secondary"
                    onClick={close}
                    outline
                  />
                  <Button
                    text="operationDialog.previousStep"
                    color="secondary"
                    onClick={onPreviewPrevious}
                    outline
                  />
                </div>
              </>
            );
          } else if (state === "preview") {
            return (
              <>
                <div className="container">
                  {createPreviewRow()}
                  <div className="row">
                    {t("settings.dialogChangeAvatar.prompt.preview")}
                  </div>
                </div>
                <hr />
                <div className="cru-dialog-bottom-area">
                  <Button
                    text="operationDialog.cancel"
                    color="secondary"
                    outline
                    onClick={close}
                  />
                  <Button
                    text="operationDialog.previousStep"
                    color="secondary"
                    outline
                    onClick={onPreviewPrevious}
                  />
                  <Button
                    text="settings.dialogChangeAvatar.upload"
                    color="primary"
                    onClick={upload}
                  />
                </div>
              </>
            );
          } else if (state === "uploading") {
            return (
              <>
                <div className="container">
                  {createPreviewRow()}
                  <div className="row">
                    {t("settings.dialogChangeAvatar.prompt.uploading")}
                  </div>
                </div>
              </>
            );
          } else if (state === "success") {
            return (
              <>
                <div className="container">
                  <div className="row p-4 text-success">
                    {t("operationDialog.success")}
                  </div>
                </div>
                <hr />
                <div className="cru-dialog-bottom-area">
                  <Button
                    text="operationDialog.ok"
                    color="success"
                    onClick={close}
                  />
                </div>
              </>
            );
          } else {
            return (
              <>
                <div className="container">
                  {createPreviewRow()}
                  <div className="row text-danger">{trueMessage}</div>
                </div>
                <hr />
                <div>
                  <Button
                    text="operationDialog.cancel"
                    color="secondary"
                    onClick={close}
                  />
                  <Button
                    text="operationDialog.retry"
                    color="primary"
                    onClick={upload}
                  />
                </div>
              </>
            );
          }
        })()}
      </DialogContainer>
    </Dialog>
  );
}
