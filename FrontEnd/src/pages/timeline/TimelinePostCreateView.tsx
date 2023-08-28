import { useState, useEffect, ChangeEventHandler } from "react";
import { useTranslation } from "react-i18next";

import { UiLogicError } from "~src/common";

import {
  getHttpTimelineClient,
  HttpTimelineInfo,
  HttpTimelinePostInfo,
  HttpTimelinePostPostRequestData,
} from "~src/http/timeline";

import { pushAlert } from "~src/services/alert";

import base64 from "~src/utilities/base64";

import BlobImage from "~src/components/BlobImage";
import LoadingButton from "~src/components/button/LoadingButton";
import PopupMenu from "~src/components/menu/PopupMenu";
import MarkdownPostEdit from "./MarkdownPostEdit";
import TimelinePostCard from "./TimelinePostCard";
import TimelinePostContainer from "./TimelinePostContainer";
import IconButton from "~src/components/button/IconButton";

import "./TimelinePostCreateView.css";
import classNames from "classnames";

interface TimelinePostEditTextProps {
  text: string;
  disabled: boolean;
  onChange: (text: string) => void;
  className?: string;
}

function TimelinePostEditText(props: TimelinePostEditTextProps) {
  const { text, disabled, onChange, className } = props;

  return (
    <textarea
      value={text}
      disabled={disabled}
      onChange={(event) => {
        onChange(event.target.value);
      }}
      className={classNames("timeline-post-create-edit-text", className)}
    />
  );
}

interface TimelinePostEditImageProps {
  onSelect: (file: File | null) => void;
  disabled: boolean;
}

function TimelinePostEditImage(props: TimelinePostEditImageProps) {
  const { onSelect, disabled } = props;

  const { t } = useTranslation();

  const [file, setFile] = useState<File | null>(null);
  const [error, setError] = useState<boolean>(false);

  const onInputChange: ChangeEventHandler<HTMLInputElement> = (e) => {
    setError(false);
    const files = e.target.files;
    if (files == null || files.length === 0) {
      setFile(null);
      onSelect(null);
    } else {
      setFile(files[0]);
    }
  };

  useEffect(() => {
    return () => {
      onSelect(null);
    };
  }, [onSelect]);

  return (
    <>
      <input
        type="file"
        onChange={onInputChange}
        accept="image/*"
        disabled={disabled}
        className="mx-3 my-1"
      />
      {file != null && !error && (
        <BlobImage
          src={file}
          className="timeline-post-create-image"
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
}

type PostKind = "text" | "markdown" | "image";

const postKindIconMap: Record<PostKind, string> = {
  text: "fonts",
  markdown: "markdown",
  image: "image",
};

export interface TimelinePostEditProps {
  className?: string;
  timeline: HttpTimelineInfo;
  onPosted: (newPost: HttpTimelinePostInfo) => void;
}

function TimelinePostEdit(props: TimelinePostEditProps) {
  const { timeline, className, onPosted } = props;

  const { t } = useTranslation();

  const [process, setProcess] = useState<boolean>(false);

  const [kind, setKind] = useState<Exclude<PostKind, "markdown">>("text");
  const [showMarkdown, setShowMarkdown] = useState<boolean>(false);

  const [text, setText] = useState<string>("");
  const [image, setImage] = useState<File | null>(null);

  const draftTextLocalStorageKey = `timeline.${timeline.owner.username}.${timeline.nameV2}.postDraft.text`;

  useEffect(() => {
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
            "Content type is image but image blob is null.",
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
      .postPost(timeline.owner.username, timeline.nameV2, {
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
        () => {
          setProcess(false);
          onPostError();
        },
      );
  };

  return (
    <TimelinePostContainer
      className={classNames(className, "timeline-post-create-container")}
    >
      <TimelinePostCard className="timeline-post-create-card">
        {showMarkdown ? (
          <MarkdownPostEdit
            className="cru-fill-parent"
            onClose={() => setShowMarkdown(false)}
            owner={timeline.owner.username}
            timeline={timeline.nameV2}
            onPosted={onPosted}
            onPostError={onPostError}
          />
        ) : (
          <div className="timeline-post-create">
            <div className="timeline-post-create-edit-area">
              {(() => {
                if (kind === "text") {
                  return (
                    <TimelinePostEditText
                      className="timeline-post-create-edit-text"
                      text={text}
                      disabled={process}
                      onChange={(text) => {
                        setText(text);
                        window.localStorage.setItem(
                          draftTextLocalStorageKey,
                          text,
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
            </div>
            <div className="timeline-post-create-right-area">
              <PopupMenu
                containerClassName="timeline-post-create-kind-select"
                items={(["text", "image", "markdown"] as const).map((kind) => ({
                  type: "button",
                  text: `timeline.post.type.${kind}`,
                  iconClassName: postKindIconMap[kind],
                  onClick: () => {
                    if (kind === "markdown") {
                      setShowMarkdown(true);
                    } else {
                      setKind(kind);
                    }
                  },
                }))}
              >
                <IconButton color="primary" icon={postKindIconMap[kind]} />
              </PopupMenu>
              <LoadingButton
                onClick={() => void onSend()}
                color="primary"
                disabled={!canSend}
                loading={process}
              >
                {t("timeline.send")}
              </LoadingButton>
            </div>
          </div>
        )}
      </TimelinePostCard>
    </TimelinePostContainer>
  );
}

export default TimelinePostEdit;
