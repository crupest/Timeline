#!/usr/bin/env ts-node

/**
 * Color variable name scheme:
 * no variant: --[prefix]-[name]-color: [color];
 * with variant: --[prefix]-[name]-[variant]-color: [color];
 *
 * Lightness variants come from material design (https://m3.material.io/styles/color/the-color-system/tokens)
 */

import { stdout } from "process";

interface CssSegment {
  toCssString(): string;
}

interface Color extends CssSegment {
  readonly type: "hsl" | "css-var";
  toString(): string;
}

class HslColor implements Color {
  readonly type = "hsl";

  constructor(
    public h: number,
    public s: number,
    public l: number,
  ) {}

  withLightness(lightness: number): HslColor {
    return new HslColor(this.h, this.s, lightness);
  }

  toCssString(): string {
    return this.toString();
  }

  toString(): string {
    return `hsl(${this.h} ${this.s}% ${this.l}%)`;
  }

  static readonly white = new HslColor(0, 0, 100);
  static readonly black = new HslColor(0, 0, 0);
}

class ColorVariable implements CssSegment {
  constructor(
    public prefix: string,
    public name: string,
    public variant: string,
  ) {}

  toString(): string {
    const variantPart = this.variant !== "" ? `-${this.variant}` : "";
    return `--${this.prefix}-${this.name}${variantPart}-color`;
  }

  toCssString(): string {
    return this.toString();
  }
}

class CssVarColor implements Color {
  readonly type = "css-var";

  constructor(public colorVariable: ColorVariable) {}

  toCssString(): string {
    return this.toString();
  }

  toString(): string {
    return `var(${this.colorVariable.toString()})`;
  }
}

class ColorVariableDefinition implements CssSegment {
  constructor(
    public variable: ColorVariable,
    public color: Color,
  ) {}

  toCssString(): string {
    return `${this.variable.toCssString()}: ${this.color.toCssString()};`;
  }
}

abstract class ColorGroup implements CssSegment {
  abstract getColorVariables(): ColorVariableDefinition[];
  toCssString(): string {
    return this.getColorVariables()
      .map((c) => c.toCssString())
      .join("\n");
  }
}

interface LightnessVariantInfo {
  name: string;
  lightness: number;
}

class LightnessVariantColorGroup extends ColorGroup {
  constructor(
    public prefix: string,
    public name: string,
    public baseColor: HslColor,
    public variants: LightnessVariantInfo[],
  ) {
    super();
  }

  getColorVariables(): ColorVariableDefinition[] {
    const result: ColorVariableDefinition[] = [];

    for (const variant of this.variants) {
      const color = this.baseColor.withLightness(variant.lightness);
      result.push(
        new ColorVariableDefinition(
          new ColorVariable(this.prefix, this.name, variant.name),
          color,
        ),
      );
    }

    return result;
  }
}

class VarAliasColorGroup extends ColorGroup {
  constructor(
    public prefix: string,
    public newName: string,
    public oldName: string,
    public variants: string[],
  ) {
    super();
  }

  getColorVariables(): ColorVariableDefinition[] {
    const result = [];
    for (const variant of this.variants) {
      result.push(
        new ColorVariableDefinition(
          new ColorVariable(this.prefix, this.newName, variant),
          new CssVarColor(
            new ColorVariable(this.prefix, this.oldName, variant),
          ),
        ),
      );
    }
    return result;
  }
}

class CompositeColorGroup extends ColorGroup {
  constructor(public groups: ColorGroup[]) {
    super();
  }

  getColorVariables(): ColorVariableDefinition[] {
    return this.groups
      .map((g) => g.getColorVariables())
      .reduce((prev, curr) => prev.concat(curr), []);
  }
}

interface ThemeColorsInfo {
  keyColors: { name: string; color: HslColor }[];
  neutralColor: HslColor;
}

type ColorMode = "light" | "dark";

type ThemeColorVariantLightnessVariantsInfo =
  | number
  | number[]
  | {
      base: number;
      direction: "darker" | "lighter";
      levels: number;
      step: number;
    };

interface ThemeColorVariantInfo {
  name: string;
  variants: {
    light: ThemeColorVariantLightnessVariantsInfo;
    dark: ThemeColorVariantLightnessVariantsInfo;
  };
}

class ThemeColorVariant {
  constructor(
    public name: string,
    public variants: {
      light: ThemeColorVariantLightnessVariantsInfo;
      dark: ThemeColorVariantLightnessVariantsInfo;
    },
  ) {}
  getLightnessVariants(mode: ColorMode): LightnessVariantInfo[] {
    const { name, variants } = this;
    const list = variants[mode];

    function variantName(i: number) {
      if (name.length === 0) {
        return i === 0 ? "" : String(i);
      } else {
        return i === 0 ? name : `${name}-${i}`;
      }
    }

    function fromList(list: number[]): LightnessVariantInfo[] {
      return list.map((l, i) => ({
        name: variantName(i),
        lightness: l,
      }));
    }

    if (typeof list === "number") {
      return fromList([list]);
    } else if (Array.isArray(list)) {
      return fromList(list);
    } else {
      const l = [list.base];
      for (let i = 1; i <= list.levels; i++) {
        if (list.direction === "darker") {
          l.push(list.base - i * list.step);
        } else {
          l.push(list.base + i * list.step);
        }
      }
      return fromList(l);
    }
  }

  static from(info: ThemeColorVariantInfo): ThemeColorVariant {
    return new ThemeColorVariant(info.name, info.variants);
  }
}

class ThemeColor {
  variants: ThemeColorVariant[];

  constructor(
    public prefix: string,
    public name: string,
    public color: HslColor,
    variants: ThemeColorVariantInfo[],
  ) {
    this.variants = variants.map((v) => ThemeColorVariant.from(v));
  }

  getLightnessVariants(mode: ColorMode): LightnessVariantInfo[] {
    return this.variants.flatMap((v) => v.getLightnessVariants(mode));
  }

  getLightnessVariantColorGroup(mode: ColorMode): LightnessVariantColorGroup {
    return new LightnessVariantColorGroup(
      this.prefix,
      this.name,
      this.color,
      this.getLightnessVariants(mode),
    );
  }
}

class Theme {
  static keyColorVariants: ThemeColorVariantInfo[] = [
    {
      name: "",
      variants: {
        light: [40, 37, 34],
        dark: [80, 75, 68],
      },
    },
    {
      name: "on",
      variants: {
        light: 100,
        dark: 20,
      },
    },
    {
      name: "container",
      variants: {
        light: [90, 80, 70],
        dark: [30, 25, 20],
      },
    },
    {
      name: "on-container",
      variants: {
        light: 10,
        dark: 90,
      },
    },
  ];

  static surfaceColorVariants: ThemeColorVariantInfo[] = [
    {
      name: "dim",
      variants: {
        light: 87,
        dark: 6,
      },
    },
    {
      name: "",
      variants: {
        light: [98, 90, 82],
        dark: [6, 25, 40],
      },
    },
    {
      name: "bright",
      variants: {
        light: 98,
        dark: 24,
      },
    },
    {
      name: "container-lowest",
      variants: {
        light: 100,
        dark: 4,
      },
    },
    {
      name: "container-low",
      variants: {
        light: 96,
        dark: 10,
      },
    },
    {
      name: "container",
      variants: {
        light: 94,
        dark: 12,
      },
    },
    {
      name: "container-high",
      variants: {
        light: 92,
        dark: 17,
      },
    },
    {
      name: "container-highest",
      variants: {
        light: 90,
        dark: 22,
      },
    },
    {
      name: "on",
      variants: {
        light: 10,
        dark: 90,
      },
    },
    {
      name: "on-variant",
      variants: {
        light: 30,
        dark: 80,
      },
    },
    {
      name: "outline",
      variants: {
        light: 50,
        dark: 60,
      },
    },
    {
      name: "outline-variant",
      variants: {
        light: 80,
        dark: 30,
      },
    },
  ];

  constructor(
    public prefix: string,
    public themeColors: ThemeColorsInfo,
  ) {}

  getColorModeColorDefinitions(mode: ColorMode): ColorGroup {
    const groups: ColorGroup[] = [];
    for (const { name, color } of this.themeColors.keyColors) {
      const themeColor = new ThemeColor(
        this.prefix,
        name,
        color,
        Theme.keyColorVariants,
      );
      groups.push(themeColor.getLightnessVariantColorGroup(mode));
    }
    const neutralThemeColor = new ThemeColor(
      this.prefix,
      "surface",
      this.themeColors.neutralColor,
      Theme.surfaceColorVariants,
    );
    groups.push(neutralThemeColor.getLightnessVariantColorGroup(mode));
    return new CompositeColorGroup(groups);
  }

  getAliasColorDefinitions(name: string): ColorGroup {
    const sampleThemeColor = this.themeColors.keyColors[0];
    const themeColor = new ThemeColor(
      this.prefix,
      sampleThemeColor.name,
      sampleThemeColor.color,
      Theme.keyColorVariants,
    );
    const sampleMode = "light";
    return new VarAliasColorGroup(
      this.prefix,
      "key",
      name,
      themeColor.getLightnessVariants(sampleMode).map((v) => v.name),
    );
  }

  generateCss(print: (text: string, indent: number) => void): void {
    print(":root {", 0);
    print(this.getColorModeColorDefinitions("light").toCssString(), 1);
    print("}", 0);

    print("", 0);

    print("@media (prefers-color-scheme: dark) {", 0);
    print(":root {", 1);
    print(this.getColorModeColorDefinitions("dark").toCssString(), 2);
    print("}", 1);
    print("}", 0);

    print("", 0);

    for (const { name } of this.themeColors.keyColors) {
      print(`.${this.prefix}-${name} {`, 0);
      print(this.getAliasColorDefinitions(name).toCssString(), 1);
      print("}", 0);

      print("", 0);
    }
  }
}

(function main() {
  const prefix = "cru";
  const themeColors: ThemeColorsInfo = {
    keyColors: [
      { name: "primary", color: new HslColor(210, 100, 50) },
      { name: "secondary", color: new HslColor(40, 100, 50) },
      { name: "tertiary", color: new HslColor(160, 100, 50) },
      { name: "danger", color: new HslColor(0, 100, 50) },
      { name: "success", color: new HslColor(120, 60, 50) },
    ],
    neutralColor: new HslColor(0, 0, 50),
  };

  const theme = new Theme(prefix, themeColors);

  let output = "";

  function indentText(
    text: string,
    level: number,
    indentWidth = 2,
    appendNewlines = 1,
  ): string {
    const lines = text.split("\n");
    const indent = " ".repeat(level * indentWidth);
    return (
      lines
        .map((line) => (line.length === 0 ? "" : `${indent}${line}`))
        .join("\n") + "\n".repeat(appendNewlines)
    );
  }

  function print(text: string, indent = 0, appendNewlines = 1) {
    output += indentText(text, indent, 2, appendNewlines);
  }

  print("/* Generated by theme-generator.ts */\n");
  theme.generateCss(print);

  stdout.write(output);
})();
