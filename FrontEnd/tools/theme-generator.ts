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
  variant: string;
  lightness: number;
}

class LightnessVariantColorGroup extends ColorGroup {
  constructor(
    public prefix: string,
    public name: string,
    public baseColor: HslColor,
    public lightnessVariants: LightnessVariantInfo[],
  ) {
    super();
  }

  getColorVariables(): ColorVariableDefinition[] {
    const result: ColorVariableDefinition[] = [];

    for (const lightnessVariant of this.lightnessVariants) {
      const color = this.baseColor.withLightness(lightnessVariant.lightness);
      result.push(
        new ColorVariableDefinition(
          new ColorVariable(this.prefix, this.name, lightnessVariant.variant),
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

interface ThemeColors {
  keyColors: { name: string; color: HslColor }[];
  neutralColor: HslColor;
}

type ColorMode = "light" | "dark";

interface ColorModeColorVariant {
  variant: string;
  lightness: { light: number; dark: number };
}

class Theme {
  static keyColorVariants: ColorModeColorVariant[] = [
    {
      variant: "",
      lightness: {
        light: 40,
        dark: 80,
      },
    },
    {
      variant: "on",
      lightness: {
        light: 100,
        dark: 20,
      },
    },
    {
      variant: "container",
      lightness: {
        light: 90,
        dark: 30,
      },
    },
    {
      variant: "on-container",
      lightness: {
        light: 10,
        dark: 90,
      },
    },
  ];

  static surfaceColorVariants: ColorModeColorVariant[] = [
    {
      variant: "dim",
      lightness: {
        light: 87,
        dark: 6,
      },
    },
    {
      variant: "",
      lightness: {
        light: 98,
        dark: 6,
      },
    },
    {
      variant: "bright",
      lightness: {
        light: 98,
        dark: 24,
      },
    },
    {
      variant: "container-lowest",
      lightness: {
        light: 100,
        dark: 4,
      },
    },
    {
      variant: "container-low",
      lightness: {
        light: 96,
        dark: 10,
      },
    },
    {
      variant: "container",
      lightness: {
        light: 94,
        dark: 12,
      },
    },
    {
      variant: "container-high",
      lightness: {
        light: 92,
        dark: 17,
      },
    },
    {
      variant: "container-highest",
      lightness: {
        light: 90,
        dark: 22,
      },
    },
    {
      variant: "on",
      lightness: {
        light: 10,
        dark: 90,
      },
    },
    {
      variant: "on-variant",
      lightness: {
        light: 30,
        dark: 80,
      },
    },
    {
      variant: "outline",
      lightness: {
        light: 50,
        dark: 60,
      },
    },
    {
      variant: "outline-variant",
      lightness: {
        light: 80,
        dark: 30,
      },
    },
  ];

  static getLightnessVariants(
    mode: ColorMode,
    colorModeColorVariants: ColorModeColorVariant[],
  ): LightnessVariantInfo[] {
    return colorModeColorVariants.map((v) => ({
      variant: v.variant,
      lightness: v.lightness[mode],
    }));
  }

  constructor(
    public prefix: string,
    public themeColors: ThemeColors,
    public levels = 3,
  ) {}

  getColorModeColorDefinitions(mode: ColorMode): ColorGroup {
    const groups: ColorGroup[] = [];
    for (const { name, color } of this.themeColors.keyColors) {
      const colorGroup = new LightnessVariantColorGroup(
        this.prefix,
        name,
        color,
        Theme.getLightnessVariants(mode, Theme.keyColorVariants),
      );
      groups.push(colorGroup);
    }
    groups.push(
      new LightnessVariantColorGroup(
        this.prefix,
        "surface",
        this.themeColors.neutralColor,
        Theme.getLightnessVariants(mode, Theme.surfaceColorVariants),
      ),
    );
    return new CompositeColorGroup(groups);
  }

  getAliasColorDefinitions(name: string): ColorGroup {
    return new VarAliasColorGroup(
      this.prefix,
      "key",
      name,
      Theme.keyColorVariants.map((v) => v.variant),
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
  const themeColors: ThemeColors = {
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
