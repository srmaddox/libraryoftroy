/**
 * A TypeScript implementation of a GUID (Globally Unique Identifier) compatible with .NET's Guid
 */
export class Guid {
  private static readonly EMPTY = '00000000-0000-0000-0000-000000000000';
  private static readonly REGEX = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;

  private readonly value: string;

  /**
   * Constructor creates a new Guid instance
   * @param value Optional guid string value
   */
  constructor(value?: string) {
    if (!value) {
      this.value = Guid.EMPTY;
    } else {
      const normalized = value.trim().toLowerCase();
      if (!Guid.isValid(normalized)) {
        throw new Error('Invalid GUID format');
      }
      this.value = normalized;
    }
  }

  /**
   * Returns a new Guid with a random value
   */
  public static newGuid(): Guid {
    // Implementation follows RFC4122 version 4
    const bytes = new Uint8Array(16);

    // Fill with random values
    crypto.getRandomValues(bytes);

    // Set the version bits (bits 6-7 of 8th byte to 0b01 == version 4)
    bytes[6] = (bytes[6] & 0x0f) | 0x40;

    // Set the variant bits (bits 6-7 of 10th byte to 0b10)
    bytes[8] = (bytes[8] & 0x3f) | 0x80;

    // Format the bytes to match the GUID format
    const segments = [
      bytes.slice(0, 4),
      bytes.slice(4, 6),
      bytes.slice(6, 8),
      bytes.slice(8, 10),
      bytes.slice(10)
    ];

    // Convert bytes to hex string with proper formatting
    const hexStrings = segments.map(segment =>
      Array.from(segment)
        .map(b => b.toString(16).padStart(2, '0'))
        .join('')
    );

    return new Guid(
      `${hexStrings[0]}-${hexStrings[1]}-${hexStrings[2]}-${hexStrings[3]}-${hexStrings[4]}`
    );
  }

  /**
   * Creates an empty (zero) Guid
   */
  public static empty(): Guid {
    return new Guid(Guid.EMPTY);
  }

  /**
   * Parses a string into a Guid, returning null if invalid
   */
  public static parse(input: string): Guid | null {
    try {
      return new Guid(input);
    } catch {
      return null;
    }
  }

  /**
   * Tries to parse a string into a Guid
   * @returns true if successful, false otherwise
   */
  public static tryParse(input: string, output: { guid: Guid | null }): boolean {
    try {
      output.guid = new Guid(input);
      return true;
    } catch {
      output.guid = null;
      return false;
    }
  }

  /**
   * Checks if a string is a valid GUID format
   */
  public static isValid(value: string): boolean {
    return Guid.REGEX.test(value);
  }

  /**
   * Returns the string value of the Guid
   */
  public toString(): string {
    return this.value;
  }

  /**
   * Returns the Guid as a 32-character hex string without dashes
   */
  public toHexString(): string {
    return this.value.replace(/-/g, '');
  }

  /**
   * Equality comparison with another Guid
   */
  public equals(other: Guid | string | null): boolean {
    if (other === null || other === undefined) {
      return false;
    }

    if (typeof other === 'string') {
      try {
        other = new Guid(other);
      } catch {
        return false;
      }
    }

    return this.value === other.value;
  }

  /**
   * Creates a new array filled with random byte values
   */
  public toByteArray(): Uint8Array {
    const hex = this.toHexString();
    const bytes = new Uint8Array(16);

    for (let i = 0; i < 16; i++) {
      bytes[i] = parseInt(hex.substr(i * 2, 2), 16);
    }

    return bytes;
  }

  /**
   * Creates a Guid from a byte array
   */
  public static fromByteArray(bytes: Uint8Array): Guid {
    if (bytes.length !== 16) {
      throw new Error('Byte array must be 16 bytes long');
    }

    const hexStrings = [
      Array.from(bytes.slice(0, 4)),
      Array.from(bytes.slice(4, 6)),
      Array.from(bytes.slice(6, 8)),
      Array.from(bytes.slice(8, 10)),
      Array.from(bytes.slice(10))
    ].map(segment =>
      segment.map(b => b.toString(16).padStart(2, '0')).join('')
    );

    return new Guid(
      `${hexStrings[0]}-${hexStrings[1]}-${hexStrings[2]}-${hexStrings[3]}-${hexStrings[4]}`
    );
  }

  /**
   * Converts the Guid to JSON format
   */
  public toJSON(): string {
    return this.toString();
  }
}
