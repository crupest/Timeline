function clamp(value: number, min: number, max: number): number {
  return Math.min(Math.max(value, min), max);
}

export interface Point {
  x: number;
  y: number;
}

export type Movement = Point;

export interface Size {
  width: number;
  height: number;
}

export class Rect {
  static empty = new Rect(0, 0, 0, 0);
  static max = new Rect(
    Number.MIN_VALUE,
    Number.MIN_VALUE,
    Number.MAX_VALUE,
    Number.MAX_VALUE,
  );

  static from({
    left,
    top,
    width,
    height,
  }: {
    left: number;
    top: number;
    width: number;
    height: number;
  }): Rect {
    return new Rect(left, top, width, height);
  }

  constructor(
    public left: number,
    public top: number,
    public width: number,
    public height: number,
  ) {}

  get right(): number {
    return this.left + this.width;
  }

  set right(value: number) {
    this.width = this.left + value;
  }

  get bottom(): number {
    return this.top + this.height;
  }

  set bottom(value: number) {
    this.height = this.top + value;
  }

  get ratio(): number {
    return this.height / this.width;
  }

  get position(): Point {
    return {
      x: this.left,
      y: this.top,
    };
  }

  set position(value: Point) {
    this.left = value.x;
    this.top = value.y;
  }

  get size(): Size {
    return {
      width: this.width,
      height: this.height,
    };
  }

  set size(value: Size) {
    this.width = value.width;
    this.height = value.height;
  }

  get normalizedLeft(): number {
    return this.width >= 0 ? this.left : this.right;
  }

  get normalizedTop(): number {
    return this.height >= 0 ? this.top : this.bottom;
  }

  get normalizedRight(): number {
    return this.width >= 0 ? this.right : this.left;
  }

  get normalizedBottom(): number {
    return this.height >= 0 ? this.bottom : this.top;
  }

  get normalizedWidth(): number {
    return Math.abs(this.width);
  }

  get normalizedHeight(): number {
    return Math.abs(this.height);
  }

  get normalizedSize(): Size {
    return {
      width: this.normalizedWidth,
      height: this.normalizedHeight,
    };
  }

  get normalizedRatio(): number {
    return Math.abs(this.ratio);
  }

  normalize(): Rect {
    if (this.width < 0) {
      this.width = -this.width;
      this.left -= this.width;
    }
    if (this.height < 0) {
      this.height = -this.height;
      this.top -= this.height;
    }
    return this;
  }

  move(movement: Movement): Rect {
    this.left += movement.x;
    this.top += movement.y;
    return this;
  }

  expand(size: Size | Point): Rect {
    if ("x" in size) {
      this.width += size.x;
      this.height += size.y;
    } else {
      this.width += size.width;
      this.height += size.height;
    }
    return this;
  }

  copy(): Rect {
    return new Rect(this.left, this.top, this.width, this.height);
  }
}

export function adjustRectToContainer(
  rect: Rect,
  container: Rect,
  mode: "move" | "resize" | "both",
  options?: {
    targetRatio?: number;
    resizeNoFlip?: boolean;
    ratioCorrectBasedOn?: "bigger" | "smaller" | "width" | "height";
  },
): Rect {
  rect = rect.copy();
  container = container.copy().normalize();

  if (process.env.NODE_ENV === "development") {
    if (mode === "move") {
      if (rect.normalizedWidth > container.width) {
        console.warn(
          "adjust rect (move): rect.normalizedWidth > container.normalizedWidth",
        );
      }
      if (rect.normalizedHeight > container.height) {
        console.warn(
          "adjust rect (move): rect.normalizedHeight > container.normalizedHeight",
        );
      }
    }
    if (mode === "resize") {
      if (rect.left < container.left) {
        console.warn(
          "adjust rect (resize): rect.left < container.normalizedLeft",
        );
      }
      if (rect.left > container.right) {
        console.warn(
          "adjust rect (resize): rect.left > container.normalizedRight",
        );
      }
      if (rect.top < container.top) {
        console.warn(
          "adjust rect (resize): rect.top < container.normalizedTop",
        );
      }
      if (rect.top > container.bottom) {
        console.warn(
          "adjust rect (resize): rect.top > container.normalizedBottom",
        );
      }
    }
  }

  if (mode === "move") {
    rect.left =
      rect.width >= 0
        ? clamp(rect.left, container.left, container.right - rect.width)
        : clamp(rect.left, container.left - rect.width, container.right);
    rect.top =
      rect.height >= 0
        ? clamp(rect.top, container.top, container.bottom - rect.height)
        : clamp(rect.top, container.top - rect.height, container.bottom);
  } else if (mode === "resize") {
    const noFlip = options?.resizeNoFlip;
    rect.right = clamp(
      rect.right,
      noFlip ? 0 : container.left,
      container.right,
    );
    rect.bottom = clamp(
      rect.bottom,
      noFlip ? 0 : container.top,
      container.bottom,
    );
  } else {
    rect.left = clamp(rect.left, container.left, container.right);
    rect.top = clamp(rect.top, container.top, container.bottom);
    rect.right = clamp(rect.right, container.left, container.right);
    rect.bottom = clamp(rect.bottom, container.top, container.bottom);
  }

  // Now correct ratio
  const currentRatio = rect.normalizedRatio;
  let targetRatio = options?.targetRatio;
  if (targetRatio != null) targetRatio = Math.abs(targetRatio);
  if (targetRatio != null && currentRatio !== targetRatio) {
    const { ratioCorrectBasedOn } = options ?? {};

    const newWidth =
      (Math.sign(rect.width) * rect.normalizedHeight) / targetRatio;
    const newHeight =
      Math.sign(rect.height) * rect.normalizedWidth * targetRatio;

    const newBottom = rect.top + newHeight;
    const newRight = rect.left + newWidth;

    if (ratioCorrectBasedOn === "width") {
      if (newBottom >= container.top && newBottom <= container.bottom) {
        rect.height = newHeight;
      } else {
        rect.bottom = clamp(newBottom, container.top, container.bottom);
        rect.width =
          (Math.sign(rect.width) * rect.normalizedHeight) / targetRatio;
      }
    } else if (ratioCorrectBasedOn === "height") {
      if (newRight >= container.left && newRight <= container.right) {
        rect.width = newWidth;
      } else {
        rect.right = clamp(newRight, container.left, container.right);
        rect.height =
          Math.sign(rect.height) * rect.normalizedWidth * targetRatio;
      }
    } else if (ratioCorrectBasedOn === "smaller") {
      if (currentRatio > targetRatio) {
        // too tall
        rect.width =
          (Math.sign(rect.width) * rect.normalizedHeight) / targetRatio;
      } else {
        rect.height =
          Math.sign(rect.height) * rect.normalizedWidth * targetRatio;
      }
    } else {
      if (currentRatio < targetRatio) {
        // too wide
        rect.width =
          (Math.sign(rect.width) * rect.normalizedHeight) / targetRatio;
      } else {
        rect.height =
          Math.sign(rect.height) * rect.normalizedWidth * targetRatio;
      }
    }
  }

  return rect;
}
