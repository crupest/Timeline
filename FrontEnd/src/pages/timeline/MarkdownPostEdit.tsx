import * as React from "react";
import classnames from "classnames";
import { useTranslation } from "react-i18next";

import { getHttpTimelineClient, HttpTimelinePostInfo } from "@/http/timeline";

import TimelinePostBuilder from "@/services/TimelinePostBuilder";

import FlatButton from "@/views/common/button/FlatButton";
import TabPages from "@/views/common/tab/TabPages";
import ConfirmDialog from "@/views/common/dialog/ConfirmDialog";
import Spinner from "@/views/common/Spinner";
import IconButton from "@/views/common/button/IconButton";

import "./MarkdownPostEdit.css";

export interface MarkdownPostEditProps {
  owner: string;
  timeline: string;
  onPosted: (post: HttpTimelinePostInfo) => void;
  onPostError: () => void;
  onClose: () => void;
  className?: string;
  style?: React.CSSProperties;
}

const MarkdownPostEdit: React.FC<MarkdownPostEditProps> = ({
  owner: ownerUsername,
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

  const [showLeaveConfirmDialog, setShowLeaveConfirmDialog] =
    React.useState<boolean>(false);

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
    window.onbeforeunload = (): unknown => {
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
      const post = await getHttpTimelineClient().postPost(
        ownerUsername,
        timelineName,
        {
          dataList,
        }
      );
      onPosted(post);
      onClose();
    } catch (e) {
      setProcess(false);
      onPostError();
    }
  };

  return (
    <>
      <TabPages
        className={className}
        style={style}
        pageContainerClassName="py-2"
        dense
        actions={
          process ? (
            <Spinner />
          ) : (
            <div>
              <IconButton
                icon="x"
                color="danger"
                large
                className="cru-align-middle me-2"
                onClick={() => {
                  if (canLeave) {
                    onClose();
                  } else {
                    setShowLeaveConfirmDialog(true);
                  }
                }}
              />
              {canSend && (
                <FlatButton text="timeline.send" onClick={() => void send()} />
              )}
            </div>
          )
        }
        pages={[
          {
            name: "text",
            text: "edit",
            page: (
              <textarea
                value={text}
                disabled={process}
                className="cru-fill-parent"
                onChange={(event) => {
                  getBuilder().setMarkdownText(event.currentTarget.value);
                }}
              />
            ),
          },
          {
            name: "images",
            text: "image",
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
                    <IconButton
                      icon="trash"
                      color="danger"
                      className={classnames(
                        "timeline-markdown-post-edit-image-delete-button",
                        process && "d-none"
                      )}
                      onClick={() => {
                        getBuilder().deleteImage(index);
                      }}
                    />
                  </div>
                ))}
                <input
                  type="file"
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
            name: "preview",
            text: "preview",
            page: (
              <div
                className="markdown-container timeline-markdown-post-edit-page"
                dangerouslySetInnerHTML={{ __html: previewHtml }}
              />
            ),
          },
        ]}
      />
      <ConfirmDialog
        onClose={() => setShowLeaveConfirmDialog(false)}
        onConfirm={onClose}
        open={showLeaveConfirmDialog}
        title="timeline.dropDraft"
        body="timeline.confirmLeave"
      />
    </>
  );
};

export default MarkdownPostEdit;
