import { useState } from "react";
import classNames from "classnames";

import { UiLogicError } from "~src/common";

import {
  getHttpTimelineClient,
  HttpTimelineInfo,
  HttpTimelinePostInfo,
  HttpTimelinePostPostRequestData,
} from "~src/http/timeline";

import base64 from "~src/utilities/base64";

import { useC } from "~/src/components/common";
import { pushAlert } from "~src/components/alert";
import { IconButton, LoadingButton } from "~src/components/button";
import PopupMenu from "~src/components/menu/PopupMenu";
import { useWindowLeave } from "~src/components/hooks";

import TimelinePostCard from "../TimelinePostCard";
import TimelinePostContainer from "../TimelinePostContainer";
import PlainTextPostEdit from "./PlainTextPostEdit";
import ImagePostEdit from "./ImagePostEdit";
import { MarkdownPostEdit, useMarkdownEdit } from "./MarkdownPostEdit";

import "./TimelinePostCreateView.css";

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

  const c = useC();

  const [process, setProcess] = useState<boolean>(false);

  const [kind, setKind] = useState<PostKind>("text");

  const draftTextLocalStorageKey = `timeline.${timeline.owner.username}.${timeline.nameV2}.postDraft.text`;
  const [text, setText] = useState<string>(
    () => window.localStorage.getItem(draftTextLocalStorageKey) ?? "",
  );
  const [image, setImage] = useState<File | null>(null);
  const {
    hasContent: mdHasContent,
    build: mdBuild,
    clear: mdClear,
    markdownEditProps,
  } = useMarkdownEdit(process);

  useWindowLeave(!mdHasContent && !image);

  const canSend =
    (kind === "text" && text.length !== 0) ||
    (kind === "image" && image != null) ||
    (kind === "markdown" && mdHasContent);

  const onPostError = (): void => {
    pushAlert({
      color: "danger",
      message: "timeline.sendPostFailed",
    });
  };

  const onSend = async (): Promise<void> => {
    setProcess(true);

    let requestDataList: HttpTimelinePostPostRequestData[];
    switch (kind) {
      case "text":
        requestDataList = [
          {
            contentType: "text/plain",
            data: await base64(text),
          },
        ];
        break;
      case "image":
        if (image == null) {
          throw new UiLogicError();
        }
        requestDataList = [
          {
            contentType: image.type,
            data: await base64(image),
          },
        ];
        break;
      case "markdown":
        if (!mdHasContent) {
          throw new UiLogicError();
        }
        requestDataList = await mdBuild();
      default:
        throw new UiLogicError("Unknown content type.");
    }

    try {
      const res = await getHttpTimelineClient().postPost(
        timeline.owner.username,
        timeline.nameV2,
        {
          dataList: requestDataList,
        },
      );

      if (kind === "text") {
        setText("");
        window.localStorage.removeItem(draftTextLocalStorageKey);
      } else if (kind === "image") {
        setImage(null);
      } else if (kind === "markdown") {
        mdClear();
      }
      onPosted(res);
    } catch (e) {
      onPostError();
    } finally {
      setProcess(false);
    }
  };

  return (
    <TimelinePostContainer
      className={classNames(className, "timeline-post-create-container")}
    >
      <TimelinePostCard className="timeline-post-create-card">
        <div className="timeline-post-create">
          <div className="timeline-post-create-edit-area">
            {kind === "text" && (
              <PlainTextPostEdit
                text={text}
                disabled={process}
                onChange={(text) => {
                  setText(text);
                  window.localStorage.setItem(draftTextLocalStorageKey, text);
                }}
              />
            )}
            {kind === "image" && (
              <ImagePostEdit
                file={image}
                onChange={setImage}
                disabled={process}
              />
            )}
            {kind === "markdown" && <MarkdownPostEdit {...markdownEditProps} />}
          </div>
          <div className="timeline-post-create-right-area">
            <PopupMenu
              containerClassName="timeline-post-create-kind-select"
              items={(["text", "image", "markdown"] as const).map((kind) => ({
                type: "button",
                text: `timeline.post.type.${kind}`,
                iconClassName: postKindIconMap[kind],
                onClick: () => {
                  setKind(kind);
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
              {c("timeline.send")}
            </LoadingButton>
          </div>
        </div>
      </TimelinePostCard>
    </TimelinePostContainer>
  );
}

export default TimelinePostEdit;
