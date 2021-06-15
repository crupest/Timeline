import React from "react";
import classnames from "classnames";
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
import { PopupMenu } from "../common/Menu";
import Card from "../common/Card";
import MarkdownPostEdit from "./MarkdownPostEdit";
import TimelineLine from "./TimelineLine";

interface TimelinePostEditTextProps {
  text: string;
  disabled: boolean;
  onChange: (text: string) => void;
  className?: string;
  style?: React.CSSProperties;
}

const TimelinePostEditText: React.FC<TimelinePostEditTextProps> = (props) => {
  const { text, disabled, onChange, className, style } = props;

  return (
    <Form.Control
      as="textarea"
      value={text}
      disabled={disabled}
      onChange={(event) => {
        onChange(event.target.value);
      }}
      className={className}
      style={style}
    />
  );
};

interface TimelinePostEditImageProps {
  onSelect: (file: File | null) => void;
  disabled: boolean;
}

const TimelinePostEditImage: React.FC<TimelinePostEditImageProps> = (props) => {
  const { onSelect, disabled } = props;

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

  React.useEffect(() => {
    return () => {
      onSelect(null);
    };
  }, [onSelect]);

  return (
    <>
      <Form.Control
        type="file"
        onChange={onInputChange}
        accept="image/*"
        disabled={disabled}
        className="mx-3 my-1"
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

type PostKind = "text" | "markdown" | "image";

const postKindIconClassNameMap: Record<PostKind, string> = {
  text: "bi-fonts",
  markdown: "bi-markdown",
  image: "bi-image",
};

export interface TimelinePostEditProps {
  className?: string;
  style?: React.CSSProperties;
  timeline: HttpTimelineInfo;
  onPosted: (newPost: HttpTimelinePostInfo) => void;
}

const TimelinePostEdit: React.FC<TimelinePostEditProps> = (props) => {
  const { timeline, style, className, onPosted } = props;

  const { t } = useTranslation();

  const [process, setProcess] = React.useState<boolean>(false);

  const [kind, setKind] = React.useState<Exclude<PostKind, "markdown">>("text");
  const [showMarkdown, setShowMarkdown] = React.useState<boolean>(false);

  const [text, setText] = React.useState<string>("");
  const [image, setImage] = React.useState<File | null>(null);

  const draftTextLocalStorageKey = `timeline.${timeline.name}.postDraft.text`;

  React.useEffect(() => {
    setText(window.localStorage.getItem(draftTextLocalStorageKey) ?? "");
  }, [draftTextLocalStorageKey]);

  const canSend =
    (kind === "text" && text.length !== 0) ||
    (kind === "image" && image != null);

  const onPostError = (): void => {
    pushAlert({
      type: "danger",
      message: "timeline.sendPostFailed",
    });
  };

  const onSend = async (): Promise<void> => {
    setProcess(true);

    let requestData: HttpTimelinePostPostRequestData;
    switch (kind) {
      case "text":
        requestData = {
          contentType: "text/plain",
          data: await base64(text),
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
            window.localStorage.removeItem(draftTextLocalStorageKey);
          }
          setProcess(false);
          setKind("text");
          onPosted(data);
        },
        (_) => {
          setProcess(false);
          onPostError();
        }
      );
  };

  return (
    <div
      className={classnames("timeline-item current", className)}
      style={style}
    >
      <TimelineLine center="node" current />
      <Card className="timeline-item-card">
        {showMarkdown ? (
          <MarkdownPostEdit
            className="w-100"
            onClose={() => setShowMarkdown(false)}
            timeline={timeline.name}
            onPosted={onPosted}
            onPostError={onPostError}
          />
        ) : (
          <Row>
            <Col className="px-1 py-1">
              {(() => {
                if (kind === "text") {
                  return (
                    <TimelinePostEditText
                      className="w-100 h-100 timeline-post-edit"
                      text={text}
                      disabled={process}
                      onChange={(t) => {
                        setText(t);
                        window.localStorage.setItem(
                          draftTextLocalStorageKey,
                          t
                        );
                      }}
                    />
                  );
                } else if (kind === "image") {
                  return (
                    <TimelinePostEditImage
                      onSelect={setImage}
                      disabled={process}
                    />
                  );
                }
              })()}
            </Col>
            <Col xs="auto" className="align-self-end m-1">
              <div className="d-block text-center mt-1 mb-2">
                <PopupMenu
                  items={(["text", "image", "markdown"] as const).map(
                    (kind) => ({
                      type: "button",
                      text: `timeline.post.type.${kind}`,
                      iconClassName: postKindIconClassNameMap[kind],
                      onClick: () => {
                        if (kind === "markdown") {
                          setShowMarkdown(true);
                        } else {
                          setKind(kind);
                        }
                      },
                    })
                  )}
                >
                  <i
                    className={classnames(
                      postKindIconClassNameMap[kind],
                      "icon-button large"
                    )}
                  />
                </PopupMenu>
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
        )}
      </Card>
    </div>
  );
};

export default TimelinePostEdit;
