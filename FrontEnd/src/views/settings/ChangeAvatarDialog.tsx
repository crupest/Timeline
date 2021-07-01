import React, { useState, useEffect } from "react";
import { useTranslation } from "react-i18next";
import { AxiosError } from "axios";

import { UiLogicError } from "@/common";

import { useUserLoggedIn } from "@/services/user";

import { getHttpUserClient } from "@/http/user";

import ImageCropper, { Clip, applyClipToImage } from "../common/ImageCropper";
import Button from "../common/button/Button";
import Dialog from "../common/dailog/Dialog";

export interface ChangeAvatarDialogProps {
  open: boolean;
  close: () => void;
}

const ChangeAvatarDialog: React.FC<ChangeAvatarDialogProps> = (props) => {
  const { t } = useTranslation();

  const user = useUserLoggedIn();

  const [file, setFile] = React.useState<File | null>(null);
  const [fileUrl, setFileUrl] = React.useState<string | null>(null);
  const [clip, setClip] = React.useState<Clip | null>(null);
  const [cropImgElement, setCropImgElement] =
    React.useState<HTMLImageElement | null>(null);
  const [resultBlob, setResultBlob] = React.useState<Blob | null>(null);
  const [resultUrl, setResultUrl] = React.useState<string | null>(null);

  const [state, setState] = React.useState<
    | "select"
    | "crop"
    | "processcrop"
    | "preview"
    | "uploading"
    | "success"
    | "error"
  >("select");

  const [message, setMessage] = useState<
    string | { type: "custom"; text: string } | null
  >("settings.dialogChangeAvatar.prompt.select");

  const trueMessage =
    message == null
      ? null
      : typeof message === "string"
      ? t(message)
      : message.text;

  const closeDialog = props.close;

  const close = React.useCallback((): void => {
    if (!(state === "uploading")) {
      closeDialog();
    }
  }, [state, closeDialog]);

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

  React.useEffect(() => {
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

  const onSelectFile = React.useCallback(
    (e: React.ChangeEvent<HTMLInputElement>): void => {
      const files = e.target.files;
      if (files == null || files.length === 0) {
        setFile(null);
      } else {
        setFile(files[0]);
      }
    },
    []
  );

  const onCropNext = React.useCallback(() => {
    if (
      cropImgElement == null ||
      clip == null ||
      clip.width === 0 ||
      file == null
    ) {
      throw new UiLogicError();
    }

    setState("processcrop");
    void applyClipToImage(cropImgElement, clip, file.type).then((b) => {
      setResultBlob(b);
    });
  }, [cropImgElement, clip, file]);

  const onCropPrevious = React.useCallback(() => {
    setFile(null);
    setState("select");
  }, []);

  const onPreviewPrevious = React.useCallback(() => {
    setResultBlob(null);
    setState("crop");
  }, []);

  const upload = React.useCallback(() => {
    if (resultBlob == null) {
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
          setMessage({ type: "custom", text: (e as AxiosError).message });
        }
      );
  }, [user.username, resultBlob]);

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
            alt={t("settings.dialogChangeAvatar.previewImgAlt")}
          />
        </div>
      </div>
    );
  };

  return (
    <Dialog open={props.open} onClose={close}>
      <h3 className="cru-color-primary">
        {t("settings.dialogChangeAvatar.title")}
      </h3>
      <hr />
      {(() => {
        if (state === "select") {
          return (
            <>
              <div className="container">
                <div className="row">
                  {t("settings.dialogChangeAvatar.prompt.select")}
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
              <hr />
              <div className="cru-dialog-bottom-area">
                <Button
                  text="operationDialog.cancel"
                  color="secondary"
                  onClick={close}
                />
              </div>
            </>
          );
        } else if (state === "crop") {
          if (fileUrl == null) {
            throw new UiLogicError();
          }
          return (
            <>
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
                  {t("settings.dialogChangeAvatar.prompt.crop")}
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
                  onClick={onCropPrevious}
                />
                <Button
                  text="operationDialog.nextStep"
                  color="primary"
                  onClick={onCropNext}
                  disabled={
                    cropImgElement == null || clip == null || clip.width === 0
                  }
                />
              </div>
            </>
          );
        } else if (state === "processcrop") {
          return (
            <>
              <div className="container">
                <div className="row">
                  {t("settings.dialogChangeAvatar.prompt.processingCrop")}
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
    </Dialog>
  );
};

export default ChangeAvatarDialog;
