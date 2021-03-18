import React from "react";
import { Form } from "react-bootstrap";
import { useTranslation } from "react-i18next";
import { Prompt } from "react-router";

import { getHttpTimelineClient, HttpTimelinePostInfo } from "@/http/timeline";

import TabPages from "../common/TabPages";
import TimelinePostBuilder from "@/services/TimelinePostBuilder";

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
          <>
            <div className="flat-button text-danger mr-2" onClick={onClose}>
              {t("operationDialog.cancel")}
            </div>
            <div className="flat-button text-primary" onClick={send}>
              {t("timeline.send")}
            </div>
          </>
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
                {images.map((image) => (
                  <img
                    key={image.url}
                    src={image.url}
                    className="timeline-markdown-post-edit-image"
                  />
                ))}
                <Form.File
                  label={t("chooseImage")}
                  accept="image/*"
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
    </>
  );
};

export default MarkdownPostEdit;
