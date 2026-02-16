export interface User {
    id: number;
    name: string;
    apiKey: string;
    encryptionKey: string;
    bypassIPWhitelist: boolean;
}
