export type Mock<T> = {
  [P in keyof T]: T[P] extends Function ? T[P] : T[P] | Mock<T[P]>;
};

export type PartialMock<T> = {
  [P in keyof T]?: T[P] extends Function ? T[P] : T[P] | PartialMock<T[P]> | Mock<T[P]>;
};
