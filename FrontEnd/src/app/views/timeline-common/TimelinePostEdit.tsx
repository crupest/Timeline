import React from "react";
import clsx from "clsx";
import { useTranslation } from "react-i18next";
import { Row, Col, Form } from "react-bootstrap";

import { UiLogicError } from "@/common";

import {
  getHttpTimelineClient,
  HttpTimelineInfo,
  HttpTimelinePostInfo,
  HttpTimelinePostPostRequestData,
} from "@/http/timeline";

import { pushAlert } from "@/services/alert";
import { base64 } from "@/http/common";

import BlobImage from "../common/BlobImage";
import LoadingButton from "../common/LoadingButton";

interface TimelinePostEditImageProps {
  onSelect: (file: File | null) => void;
}

const TimelinePostEditImage: React.FC<TimelinePostEditImageProps> = (props) => {
  const { onSelect } = props;

  const { t } = useTranslation();

  const [file, setFile] = React.useState<File | null>(null);
  const [error, setError] = React.useState<boolean>(false);

  const onInputChange: React.ChangeEventHandler<HTMLInputElement> = (e) => {
    setError(false);
    const files = e.target.files;
    if (files == null || files.length === 0) {
      setFile(null);
      onSelect(null);
    } else {
      setFile(files[0]);
    }
  };

  return (
    <>
      <Form.File
        label={t("chooseImage")}
        onChange={onInputChange}
        accept="image/*"
        className="mx-3 my-1 d-inline-block"
      />
      {file != null && !error && (
        <BlobImage
          blob={file}
          className="timeline-post-edit-image"
          onLoad={() => onSelect(file)}
          onError={() => {
            onSelect(null);
            setError(true);
          }}
        />
      )}
      {error ? <div className="text-danger">{t("loadImageError")}</div> : null}
    </>
  );
};

export interface TimelinePostEditProps {
  className?: string;
  timeline: HttpTimelineInfo;
  onPosted: (newPost: HttpTimelinePostInfo) => void;
  onHeightChange?: (height: number) => void;
}

const TimelinePostEdit: React.FC<TimelinePostEditProps> = (props) => {
  const { timeline, onHeightChange, className, onPosted } = props;

  const { t } = useTranslation();

  const [process, setProcess] = React.useState<boolean>(false);
  const [kind, setKind] = React.useState<"text" | "image">("text");
  const [text, setText] = React.useState<string>("");
  const [image, setImage] = React.useState<File | null>(null);

  const draftLocalStorageKey = `timeline.${timeline.name}.postDraft`;

  React.useEffect(() => {
    setText(window.localStorage.getItem(draftLocalStorageKey) ?? "");
  }, [draftLocalStorageKey]);

  const canSend =
    (kind === "text" && text.length !== 0) ||
    (kind === "image" && image != null);

  // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
  const containerRef = React.useRef<HTMLDivElement>(null!);

  const notifyHeightChange = (): void => {
    if (onHeightChange) {
      onHeightChange(containerRef.current.clientHeight);
    }
  };

  React.useEffect(() => {
    if (onHeightChange) {
      onHeightChange(containerRef.current.clientHeight);
    }
    return () => {
      if (onHeightChange) {
        onHeightChange(0);
      }
    };
  });

  const toggleKind = React.useCallback(() => {
    setKind((oldKind) => (oldKind === "text" ? "image" : "text"));
    setImage(null);
  }, []);

  const onSend = async (): Promise<void> => {
    setProcess(true);

    let requestData: HttpTimelinePostPostRequestData;
    switch (kind) {
      case "text":
        requestData = {
          contentType: "text/plain",
          data: await base64(new Blob([text])),
        };
        break;
      case "image":
        if (image == null) {
          throw new UiLogicError(
            "Content type is image but image blob is null."
          );
        }
        requestData = {
          contentType: image.type,
          data: await base64(image),
        };
        break;
      default:
        throw new UiLogicError("Unknown content type.");
    }

    getHttpTimelineClient()
      .postPost(timeline.name, {
        dataList: [requestData],
      })
      .then(
        (data) => {
          if (kind === "text") {
            setText("");
            window.localStorage.removeItem(draftLocalStorageKey);
          }
          setProcess(false);
          setKind("text");
          onPosted(data);
        },
        (_) => {
          pushAlert({
            type: "danger",
            message: "timeline.sendPostFailed",
          });
          setProcess(false);
        }
      );
  };

  return (
    <div
      ref={containerRef}
      className={clsx("container-fluid bg-light", className)}
    >
      <Row>
        <Col className="px-1 py-1">
          {kind === "text" ? (
            <Form.Control
              as="textarea"
              className="w-100 h-100 timeline-post-edit"
              value={text}
              disabled={process}
              onChange={(event: React.ChangeEvent<HTMLTextAreaElement>) => {
                const value = event.currentTarget.value;
                setText(value);
                window.localStorage.setItem(draftLocalStorageKey, value);
              }}
            />
          ) : (
            <TimelinePostEditImage onSelect={setImage} />
          )}
        </Col>
        <Col xs="auto" className="align-self-end m-1">
          <div className="d-block text-center mt-1 mb-2">
            <i
              onLoad={notifyHeightChange}
              className={clsx(
                kind === "text" ? "bi-image" : "bi-card-text",
                "icon-button"
              )}
              onClick={process ? undefined : toggleKind}
            />
          </div>
          <LoadingButton
            variant="primary"
            onClick={onSend}
            disabled={!canSend}
            loading={process}
          >
            {t("timeline.send")}
          </LoadingButton>
        </Col>
      </Row>
    </div>
  );
};

export default TimelinePostEdit;
