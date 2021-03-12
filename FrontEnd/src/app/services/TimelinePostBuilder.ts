import {
  escapeHtml,
  replaceEntities,
  unescapeMd,
} from "remarkable/lib/common/utils";
import { Remarkable } from "remarkable";

import { UiLogicError } from "@/common";

import { base64 } from "@/http/common";
import { HttpTimelinePostPostRequest } from "@/http/timeline";

export class TimelinePostBuilder {
  private _onChange: () => void;
  private _text = "";
  private _images: { file: File; url: string }[] = [];
  private _md: Remarkable = new Remarkable();

  constructor(onChange: () => void) {
    this._onChange = onChange;
    this._md.renderer.rules.image = ((
      _t: TimelinePostBuilder
    ): Remarkable.Rule<Remarkable.ImageToken, string> =>
      function (tokens, idx, options /*, env */) {
        const i = parseInt(tokens[idx].src);
        const src =
          ' src="' +
          (isNaN(i) && i > 0 && i <= _t._images.length
            ? escapeHtml(tokens[idx].src)
            : _t._images[i - 1].url) +
          '"';
        const title = tokens[idx].title
          ? ' title="' + escapeHtml(replaceEntities(tokens[idx].title)) + '"'
          : "";
        const alt =
          ' alt="' +
          (tokens[idx].alt
            ? escapeHtml(replaceEntities(unescapeMd(tokens[idx].alt)))
            : "") +
          '"';
        const suffix = options?.xhtmlOut ? " /" : "";
        return "<img" + src + alt + title + suffix + ">";
      })(this);
  }

  setMarkdownText(text: string): void {
    this._text = text;
    this._onChange();
  }

  appendImage(file: File): void {
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

    const [old] = this._images.splice(oldIndex, 1);
    this._images.splice(newIndex, 0, old);

    this._onChange();
  }

  deleteImage(index: number): void {
    if (index < 0 || index >= this._images.length) {
      throw new UiLogicError("Old index out of range.");
    }

    URL.revokeObjectURL(this._images[index].url);
    this._images.splice(index, 1);

    this._onChange();
  }

  get images(): { file: File; url: string }[] {
    return this._images;
  }

  renderHtml(): string {
    return this._md.render(this._text);
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
