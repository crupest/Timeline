import React, { useState, useEffect } from "react";
import { useTranslation } from "react-i18next";
import { AxiosError } from "axios";
import { Modal, Row, Button } from "react-bootstrap";

import { UiLogicError } from "@/common";

import { useUserLoggedIn } from "@/services/user";

import { getHttpUserClient } from "@/http/user";

import ImageCropper, { Clip, applyClipToImage } from "../common/ImageCropper";

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
      <Row className="justify-content-center">
        <img
          className="change-avatar-img"
          src={resultUrl}
          alt={t("settings.dialogChangeAvatar.previewImgAlt")}
        />
      </Row>
    );
  };

  return (
    <Modal show={props.open} onHide={close}>
      <Modal.Header>
        <Modal.Title> {t("settings.dialogChangeAvatar.title")}</Modal.Title>
      </Modal.Header>
      {(() => {
        if (state === "select") {
          return (
            <>
              <Modal.Body className="container">
                <Row>{t("settings.dialogChangeAvatar.prompt.select")}</Row>
                <Row>
                  <input type="file" accept="image/*" onChange={onSelectFile} />
                </Row>
              </Modal.Body>
              <Modal.Footer>
                <Button variant="secondary" onClick={close}>
                  {t("operationDialog.cancel")}
                </Button>
              </Modal.Footer>
            </>
          );
        } else if (state === "crop") {
          if (fileUrl == null) {
            throw new UiLogicError();
          }
          return (
            <>
              <Modal.Body className="container">
                <Row className="justify-content-center">
                  <ImageCropper
                    clip={clip}
                    onChange={setClip}
                    imageUrl={fileUrl}
                    imageElementCallback={setCropImgElement}
                  />
                </Row>
                <Row>{t("settings.dialogChangeAvatar.prompt.crop")}</Row>
              </Modal.Body>
              <Modal.Footer>
                <Button variant="secondary" onClick={close}>
                  {t("operationDialog.cancel")}
                </Button>
                <Button variant="secondary" onClick={onCropPrevious}>
                  {t("operationDialog.previousStep")}
                </Button>
                <Button
                  color="primary"
                  onClick={onCropNext}
                  disabled={
                    cropImgElement == null || clip == null || clip.width === 0
                  }
                >
                  {t("operationDialog.nextStep")}
                </Button>
              </Modal.Footer>
            </>
          );
        } else if (state === "processcrop") {
          return (
            <>
              <Modal.Body className="container">
                <Row>
                  {t("settings.dialogChangeAvatar.prompt.processingCrop")}
                </Row>
              </Modal.Body>
              <Modal.Footer>
                <Button variant="secondary" onClick={close}>
                  {t("operationDialog.cancel")}
                </Button>
                <Button variant="secondary" onClick={onPreviewPrevious}>
                  {t("operationDialog.previousStep")}
                </Button>
              </Modal.Footer>
            </>
          );
        } else if (state === "preview") {
          return (
            <>
              <Modal.Body className="container">
                {createPreviewRow()}
                <Row>{t("settings.dialogChangeAvatar.prompt.preview")}</Row>
              </Modal.Body>
              <Modal.Footer>
                <Button variant="secondary" onClick={close}>
                  {t("operationDialog.cancel")}
                </Button>
                <Button variant="secondary" onClick={onPreviewPrevious}>
                  {t("operationDialog.previousStep")}
                </Button>
                <Button variant="primary" onClick={upload}>
                  {t("settings.dialogChangeAvatar.upload")}
                </Button>
              </Modal.Footer>
            </>
          );
        } else if (state === "uploading") {
          return (
            <>
              <Modal.Body className="container">
                {createPreviewRow()}
                <Row>{t("settings.dialogChangeAvatar.prompt.uploading")}</Row>
              </Modal.Body>
              <Modal.Footer></Modal.Footer>
            </>
          );
        } else if (state === "success") {
          return (
            <>
              <Modal.Body className="container">
                <Row className="p-4 text-success">
                  {t("operationDialog.success")}
                </Row>
              </Modal.Body>
              <Modal.Footer>
                <Button variant="success" onClick={close}>
                  {t("operationDialog.ok")}
                </Button>
              </Modal.Footer>
            </>
          );
        } else {
          return (
            <>
              <Modal.Body className="container">
                {createPreviewRow()}
                <Row className="text-danger">{trueMessage}</Row>
              </Modal.Body>
              <Modal.Footer>
                <Button variant="secondary" onClick={close}>
                  {t("operationDialog.cancel")}
                </Button>
                <Button variant="primary" onClick={upload}>
                  {t("operationDialog.retry")}
                </Button>
              </Modal.Footer>
            </>
          );
        }
      })()}
    </Modal>
  );
};

export default ChangeAvatarDialog;
