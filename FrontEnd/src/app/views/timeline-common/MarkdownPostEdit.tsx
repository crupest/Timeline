import React from "react";
import { Nav, Form } from "react-bootstrap";
import { useTranslation } from "react-i18next";

import { getHttpTimelineClient, HttpTimelinePostInfo } from "@/http/timeline";

import TimelinePostBuilder from "@/services/TimelinePostBuilder";

export interface MarkdownPostEditProps {
  timeline: string;
  onPosted: (post: HttpTimelinePostInfo) => void;
}

const MarkdownPostEdit: React.FC<MarkdownPostEditProps> = ({
  timeline: timelineName,
  onPosted,
}) => {
  const { t } = useTranslation();

  const [tab, setTab] = React.useState<"text" | "images" | "preview">("text");

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

  const send = async (): Promise<void> => {
    const dataList = await getBuilder().build();
    const post = await getHttpTimelineClient().postPost(timelineName, {
      dataList,
    });
    onPosted(post);
  };

  return (
    <div>
      <Nav variant="tabs" className="my-2">
        <Nav.Item>
          <Nav.Link
            active={tab === "text"}
            onClick={() => {
              setTab("text");
            }}
          >
            {t("edit")}
          </Nav.Link>
        </Nav.Item>
        <Nav.Item>
          <Nav.Link
            active={tab === "images"}
            onClick={() => {
              setTab("images");
            }}
          >
            {t("image")}
          </Nav.Link>
        </Nav.Item>
        <Nav.Item>
          <Nav.Link
            active={tab === "preview"}
            onClick={() => {
              setTab("preview");
            }}
          >
            {t("preview")}
          </Nav.Link>
        </Nav.Item>
      </Nav>
      <div>
        {(() => {
          if (tab === "text") {
            return (
              <Form.Control
                as="textarea"
                value={text}
                disabled={process}
                onChange={(event) => {
                  getBuilder().setMarkdownText(event.currentTarget.value);
                }}
              />
            );
          } else if (tab === "images") {
            return <div></div>;
          } else {
            return <div dangerouslySetInnerHTML={{ __html: previewHtml }} />;
          }
        })()}
      </div>
    </div>
  );
};

export default MarkdownPostEdit;
