export class SyncStatusHub {
  private map = new Map<string, boolean>();

  get(key: string): boolean {
    return this.map.get(key) ?? false;
  }

  begin(key: string): void {
    this.map.set(key, true);
  }

  end(key: string): void {
    this.map.set(key, false);
  }
}

export const syncStatusHub = new SyncStatusHub();

export default syncStatusHub;
