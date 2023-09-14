import { useEffect, useState } from "react";
import classnames from "classnames";
import { marked } from "marked";

import { HttpTimelinePostPostRequestData } from "~src/http/timeline";

import base64 from "~src/utilities/base64";

import { array } from "~src/components/common";
import { TabPages } from "~src/components/tab";
import { IconButton } from "~src/components/button";
import BlobImage from "~src/components/BlobImage";

import "./MarkdownPostEdit.css";

class MarkedRenderer extends marked.Renderer {
  constructor(public images: string[]) {
    super();
  }

  // Custom image parser for indexed image link.
  image(href: string | null, title: string | null, text: string): string {
    if (href != null) {
      const i = parseInt(href);
      if (!isNaN(i) && i > 0 && i <= this.images.length) {
        href = this.images[i - 1];
      }
    }

    return super.image(href, title, text);
  }
}

function generateMarkedOptions(imageUrls: string[]): marked.MarkedOptions {
  return {
    mangle: false,
    headerIds: false,
    renderer: new MarkedRenderer(imageUrls),
  };
}

function renderHtml(text: string, imageUrls: string[]): string {
  return marked.parse(text, generateMarkedOptions(imageUrls));
}

async function build(
  text: string,
  images: File[],
): Promise<HttpTimelinePostPostRequestData[]> {
  return [
    {
      contentType: "text/markdown",
      data: await base64(text),
    },
    ...(await Promise.all(
      images.map(async (image) => {
        const data = await base64(image);
        return { contentType: image.type, data };
      }),
    )),
  ];
}

export function useMarkdownEdit(disabled: boolean): {
  hasContent: boolean;
  clear: () => void;
  build: () => Promise<HttpTimelinePostPostRequestData[]>;
  markdownEditProps: Omit<MarkdownPostEditProps, "className">;
} {
  const [text, setText] = useState<string>("");
  const [images, setImages] = useState<File[]>([]);

  return {
    hasContent: text !== "" || images.length !== 0,
    clear: () => {
      setText("");
      setImages([]);
    },
    build: () => {
      return build(text, images);
    },
    markdownEditProps: {
      disabled,
      text,
      images,
      onTextChange: setText,
      onImageAppend: (image) => setImages(array.copy_push(images, image)),
      onImageMove: (o, n) => setImages(array.copy_move(images, o, n)),
      onImageDelete: (i) => setImages(array.copy_delete(images, i)),
    },
  };
}

function MarkdownPreview({ text, images }: { text: string; images: File[] }) {
  const [html, setHtml] = useState("");

  useEffect(() => {
    const imageUrls = images.map((image) => URL.createObjectURL(image));

    setHtml(renderHtml(text, imageUrls));

    return () => {
      imageUrls.forEach((url) => URL.revokeObjectURL(url));
    };
  }, [text, images]);

  return (
    <div
      className="timeline-edit-markdown-preview"
      dangerouslySetInnerHTML={{ __html: html }}
    />
  );
}

interface MarkdownPostEditProps {
  disabled: boolean;
  text: string;
  images: File[];
  onTextChange: (text: string) => void;
  onImageAppend: (image: File) => void;
  onImageMove: (oldIndex: number, newIndex: number) => void;
  onImageDelete: (index: number) => void;
  className?: string;
}

export function MarkdownPostEdit({
  disabled,
  text,
  images,
  onTextChange,
  onImageAppend,
  // onImageMove,
  onImageDelete,
  className,
}: MarkdownPostEditProps) {
  return (
    <TabPages
      className={className}
      pageContainerClassName="timeline-edit-markdown-tab-page"
      dense
      pages={[
        {
          name: "text",
          text: "edit",
          page: (
            <textarea
              value={text}
              disabled={disabled}
              className="timeline-edit-markdown-text"
              onChange={(event) => {
                onTextChange(event.currentTarget.value);
              }}
            />
          ),
        },
        {
          name: "images",
          text: "image",
          page: (
            <div className="timeline-edit-markdown-images">
              {images.map((image, index) => (
                <div
                  key={image.name}
                  className="timeline-edit-markdown-image-container"
                >
                  <BlobImage src={image} />
                  <IconButton
                    icon="trash"
                    color="danger"
                    className={classnames(
                      "timeline-edit-markdown-image-delete",
                      process && "d-none",
                    )}
                    onClick={() => {
                      onImageDelete(index);
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
                    onImageAppend(files[0]);
                  }
                }}
                disabled={disabled}
              />
            </div>
          ),
        },
        {
          name: "preview",
          text: "preview",
          page: <MarkdownPreview text={text} images={images} />,
        },
      ]}
    />
  );
}
