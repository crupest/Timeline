import React from "react";
import classnames from "classnames";
import { Form, Spinner } from "react-bootstrap";
import { useTranslation } from "react-i18next";
import { Prompt } from "react-router";

import { getHttpTimelineClient, HttpTimelinePostInfo } from "@/http/timeline";

import FlatButton from "../common/FlatButton";
import TabPages from "../common/TabPages";
import TimelinePostBuilder from "@/services/TimelinePostBuilder";
import ConfirmDialog from "../common/ConfirmDialog";

export interface MarkdownPostEditProps {
  timeline: string;
  onPosted: (post: HttpTimelinePostInfo) => void;
  onPostError: () => void;
  onClose: () => void;
  className?: string;
  style?: React.CSSProperties;
}

const MarkdownPostEdit: React.FC<MarkdownPostEditProps> = ({
  timeline: timelineName,
  onPosted,
  onClose,
  onPostError,
  className,
  style,
}) => {
  const { t } = useTranslation();

  const [canLeave, setCanLeave] = React.useState<boolean>(true);

  const [process, setProcess] = React.useState<boolean>(false);

  const [
    showLeaveConfirmDialog,
    setShowLeaveConfirmDialog,
  ] = React.useState<boolean>(false);

  const [text, _setText] = React.useState<string>("");
  const [images, _setImages] = React.useState<{ file: File; url: string }[]>(
    []
  );
  const [previewHtml, _setPreviewHtml] = React.useState<string>("");

  const _builder = React.useRef<TimelinePostBuilder | null>(null);

  const getBuilder = (): TimelinePostBuilder => {
    if (_builder.current == null) {
      const builder = new TimelinePostBuilder(() => {
        setCanLeave(builder.isEmpty);
        _setText(builder.text);
        _setImages(builder.images);
        _setPreviewHtml(builder.renderHtml());
      });
      _builder.current = builder;
    }
    return _builder.current;
  };

  const canSend = text.length > 0;

  React.useEffect(() => {
    return () => {
      getBuilder().dispose();
    };
  }, []);

  React.useEffect(() => {
    window.onbeforeunload = () => {
      if (!canLeave) {
        return t("timeline.confirmLeave");
      }
    };

    return () => {
      window.onbeforeunload = null;
    };
  }, [canLeave, t]);

  const send = async (): Promise<void> => {
    setProcess(true);
    try {
      const dataList = await getBuilder().build();
      const post = await getHttpTimelineClient().postPost(timelineName, {
        dataList,
      });
      onPosted(post);
      onClose();
    } catch (e) {
      setProcess(false);
      onPostError();
    }
  };

  return (
    <>
      <Prompt when={!canLeave} message={t("timeline.confirmLeave")} />
      <TabPages
        className={className}
        style={style}
        pageContainerClassName="py-2"
        actions={
          process ? (
            <Spinner variant="primary" animation="border" size="sm" />
          ) : (
            <>
              <FlatButton
                className="me-2"
                variant="danger"
                onClick={() => {
                  if (canLeave) {
                    onClose();
                  } else {
                    setShowLeaveConfirmDialog(true);
                  }
                }}
              >
                {t("operationDialog.cancel")}
              </FlatButton>
              <FlatButton onClick={send} disabled={!canSend}>
                {t("timeline.send")}
              </FlatButton>
            </>
          )
        }
        pages={[
          {
            id: "text",
            tabText: "edit",
            page: (
              <Form.Control
                as="textarea"
                value={text}
                disabled={process}
                onChange={(event) => {
                  getBuilder().setMarkdownText(event.currentTarget.value);
                }}
              />
            ),
          },
          {
            id: "images",
            tabText: "image",
            page: (
              <div className="timeline-markdown-post-edit-page">
                {images.map((image, index) => (
                  <div
                    key={image.url}
                    className="timeline-markdown-post-edit-image-container"
                  >
                    <img
                      src={image.url}
                      className="timeline-markdown-post-edit-image"
                    />
                    <i
                      className={classnames(
                        "bi-trash text-danger icon-button timeline-markdown-post-edit-image-delete-button",
                        process && "d-none"
                      )}
                      onClick={() => {
                        getBuilder().deleteImage(index);
                      }}
                    />
                  </div>
                ))}
                <Form.File
                  label={t("chooseImage")}
                  accept="image/jpeg,image/jpg,image/png,image/gif,image/webp"
                  onChange={(event: React.ChangeEvent<HTMLInputElement>) => {
                    const { files } = event.currentTarget;
                    if (files != null && files.length !== 0) {
                      getBuilder().appendImage(files[0]);
                    }
                  }}
                  disabled={process}
                />
              </div>
            ),
          },
          {
            id: "preview",
            tabText: "preview",
            page: (
              <div
                className="markdown-container timeline-markdown-post-edit-page"
                dangerouslySetInnerHTML={{ __html: previewHtml }}
              />
            ),
          },
        ]}
      />
      {showLeaveConfirmDialog && (
        <ConfirmDialog
          onClose={() => setShowLeaveConfirmDialog(false)}
          onConfirm={onClose}
          title="timeline.dropDraft"
          body="timeline.confirmLeave"
        />
      )}
    </>
  );
};

export default MarkdownPostEdit;
