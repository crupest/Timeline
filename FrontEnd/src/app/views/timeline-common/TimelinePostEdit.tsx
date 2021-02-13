import React from "react";
import clsx from "clsx";
import { useTranslation } from "react-i18next";
import { Button, Spinner, Row, Col, Form } from "react-bootstrap";

import { UiLogicError } from "@/common";

import {
  getHttpTimelineClient,
  HttpTimelineInfo,
  HttpTimelinePostPostRequestData,
} from "@/http/timeline";

import { pushAlert } from "@/services/alert";
import { base64 } from "@/http/common";

interface TimelinePostEditImageProps {
  onSelect: (blob: Blob | null) => void;
}

const TimelinePostEditImage: React.FC<TimelinePostEditImageProps> = (props) => {
  const { onSelect } = props;
  const { t } = useTranslation();

  const [file, setFile] = React.useState<File | null>(null);
  const [fileUrl, setFileUrl] = React.useState<string | null>(null);
  const [error, setError] = React.useState<string | null>(null);

  React.useEffect(() => {
    if (file != null) {
      const url = URL.createObjectURL(file);
      setFileUrl(url);
      return () => {
        URL.revokeObjectURL(url);
      };
    }
  }, [file]);

  const onInputChange: React.ChangeEventHandler<HTMLInputElement> = React.useCallback(
    (e) => {
      const files = e.target.files;
      if (files == null || files.length === 0) {
        setFile(null);
        setFileUrl(null);
      } else {
        setFile(files[0]);
      }
      onSelect(null);
      setError(null);
    },
    [onSelect]
  );

  const onImgLoad = React.useCallback(() => {
    onSelect(file);
  }, [onSelect, file]);

  const onImgError = React.useCallback(() => {
    setError("loadImageError");
  }, []);

  return (
    <>
      <Form.File
        label={t("chooseImage")}
        onChange={onInputChange}
        accept="image/*"
        className="mx-3 my-1 d-inline-block"
      />
      {fileUrl && error == null && (
        <img
          src={fileUrl}
          className="timeline-post-edit-image"
          onLoad={onImgLoad}
          onError={onImgError}
        />
      )}
      {error != null && <div className="text-danger">{t(error)}</div>}
    </>
  );
};

export interface TimelinePostEditProps {
  className?: string;
  timeline: HttpTimelineInfo;
  onPosted: () => void;
  onHeightChange?: (height: number) => void;
}

const TimelinePostEdit: React.FC<TimelinePostEditProps> = (props) => {
  const { timeline, onHeightChange, className, onPosted } = props;

  const { t } = useTranslation();

  const [state, setState] = React.useState<"input" | "process">("input");
  const [kind, setKind] = React.useState<"text" | "image">("text");
  const [text, setText] = React.useState<string>("");
  const [imageBlob, setImageBlob] = React.useState<Blob | null>(null);

  const draftLocalStorageKey = `timeline.${timeline.name}.postDraft`;

  React.useEffect(() => {
    setText(window.localStorage.getItem(draftLocalStorageKey) ?? "");
  }, [draftLocalStorageKey]);

  const canSend = kind === "text" || (kind === "image" && imageBlob != null);

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
    setImageBlob(null);
  }, []);

  const onSend = async (): Promise<void> => {
    setState("process");

    let requestData: HttpTimelinePostPostRequestData;
    switch (kind) {
      case "text":
        requestData = {
          contentType: "text/plain",
          data: await base64(new Blob([text])),
        };
        break;
      case "image":
        if (imageBlob == null) {
          throw new UiLogicError(
            "Content type is image but image blob is null."
          );
        }
        requestData = {
          contentType: imageBlob.type,
          data: await base64(imageBlob),
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
        (_) => {
          if (kind === "text") {
            setText("");
            window.localStorage.removeItem(draftLocalStorageKey);
          }
          setState("input");
          setKind("text");
          onPosted();
        },
        (_) => {
          pushAlert({
            type: "danger",
            message: t("timeline.sendPostFailed"),
          });
          setState("input");
        }
      );
  };

  const onImageSelect = React.useCallback((blob: Blob | null) => {
    setImageBlob(blob);
  }, []);

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
              disabled={state === "process"}
              onChange={(event: React.ChangeEvent<HTMLTextAreaElement>) => {
                const value = event.currentTarget.value;
                setText(value);
                window.localStorage.setItem(draftLocalStorageKey, value);
              }}
            />
          ) : (
            <TimelinePostEditImage onSelect={onImageSelect} />
          )}
        </Col>
        <Col xs="auto" className="align-self-end m-1">
          {(() => {
            if (state === "input") {
              return (
                <>
                  <div className="d-block text-center mt-1 mb-2">
                    <i
                      onLoad={notifyHeightChange}
                      className={clsx(
                        kind === "text" ? "bi-image" : "bi-card-text",
                        "icon-button"
                      )}
                      onClick={toggleKind}
                    />
                  </div>
                  <Button
                    variant="primary"
                    onClick={onSend}
                    disabled={!canSend}
                  >
                    {t("timeline.send")}
                  </Button>
                </>
              );
            } else {
              return <Spinner variant="primary" animation="border" />;
            }
          })()}
        </Col>
      </Row>
    </div>
  );
};

export default TimelinePostEdit;
