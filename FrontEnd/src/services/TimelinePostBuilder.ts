import { marked } from "marked";

import { UiLogicError } from "@/common";

import { base64 } from "@/http/common";
import { HttpTimelinePostPostRequest } from "@/http/timeline";

class TimelinePostMarkedRenderer extends marked.Renderer {
  constructor(private _images: { file: File; url: string }[]) {
    super();
  }

  image(href: string | null, title: string | null, text: string): string {
    if (href != null) {
      const i = parseInt(href);
      if (!isNaN(i) && i > 0 && i <= this._images.length) {
        href = this._images[i - 1].url;
      }
    }
    return this.image(href, title, text);
  }
}

export default class TimelinePostBuilder {
  private _onChange: () => void;
  private _text = "";
  private _images: { file: File; url: string }[] = [];
  private _markedOptions: marked.MarkedOptions;

  constructor(onChange: () => void) {
    this._onChange = onChange;
    this._markedOptions = {
      renderer: new TimelinePostMarkedRenderer(this._images),
    };
  }

  setMarkdownText(text: string): void {
    this._text = text;
    this._onChange();
  }

  appendImage(file: File): void {
    this._images = this._images.slice();
    this._images.push({
      file,
      url: URL.createObjectURL(file),
    });
    this._onChange();
  }

  moveImage(oldIndex: number, newIndex: number): void {
    if (oldIndex < 0 || oldIndex >= this._images.length) {
      throw new UiLogicError("Old index out of range.");
    }

    if (newIndex < 0) {
      newIndex = 0;
    }

    if (newIndex >= this._images.length) {
      newIndex = this._images.length - 1;
    }

    this._images = this._images.slice();

    const [old] = this._images.splice(oldIndex, 1);
    this._images.splice(newIndex, 0, old);

    this._onChange();
  }

  deleteImage(index: number): void {
    if (index < 0 || index >= this._images.length) {
      throw new UiLogicError("Old index out of range.");
    }

    this._images = this._images.slice();

    URL.revokeObjectURL(this._images[index].url);
    this._images.splice(index, 1);

    this._onChange();
  }

  get text(): string {
    return this._text;
  }

  get images(): { file: File; url: string }[] {
    return this._images;
  }

  get isEmpty(): boolean {
    return this._text.length === 0 && this._images.length === 0;
  }

  renderHtml(): string {
    return marked.parse(this._text);
  }

  dispose(): void {
    for (const image of this._images) {
      URL.revokeObjectURL(image.url);
    }
    this._images = [];
  }

  async build(): Promise<HttpTimelinePostPostRequest["dataList"]> {
    return [
      {
        contentType: "text/markdown",
        data: await base64(this._text),
      },
      ...(await Promise.all(
        this._images.map((image) =>
          base64(image.file).then((data) => ({
            contentType: image.file.type,
            data,
          }))
        )
      )),
    ];
  }
}
