const path = require("path");

module.exports = {
  env: {
    browser: true,
    es2020: true,
  },
  extends: [
    "eslint:recommended",
    "plugin:react/recommended",
    "plugin:@typescript-eslint/eslint-recommended",
    "plugin:@typescript-eslint/recommended",
    "plugin:@typescript-eslint/recommended-requiring-type-checking",
    "plugin:prettier/recommended",
    "prettier/react",
    "prettier/@typescript-eslint",
    "plugin:react-hooks/recommended",
    "plugin:import/recommended",
    "plugin:import/typescript",
  ],
  globals: {
    Atomics: "readonly",
    SharedArrayBuffer: "readonly",
  },
  parser: "@typescript-eslint/parser",
  parserOptions: {
    project: ["./src/app/tsconfig.json", "./src/sw/tsconfig.json"],
    ecmaFeatures: {
      jsx: true,
    },
    sourceType: "module",
  },
  plugins: ["react", "@typescript-eslint", "react-hooks", "import"],
  settings: {
    react: {
      version: "detect",
    },
    "import/resolver": {
      webpack: {
        config: path.resolve(__dirname, "webpack.config.dev.js"),
      },
    },
  },
  rules: {
    "react/prop-types": "off",
    "@typescript-eslint/no-unused-vars": ["warn", { argsIgnorePattern: "^_" }],
    "@typescript-eslint/explicit-function-return-type": [
      "warn",
      {
        allowExpressions: true,
        allowTypedFunctionExpressions: true,
        allowHigherOrderFunctions: true,
      },
    ],
    "import/order": [
      "warn",
      {
        "newlines-between": "always",
      },
    ],
  },
};
