import { HubConnectionBuilder, LogLevel, HubConnection, HubConnectionState, IRetryPolicy, RetryContext } from '@microsoft/signalr';

/**Signalr reliable communication to backend server with reconnect retry policy*/
export default class CommunicationService {

    ReceiveReport: ((color: string, command: string) => void) | undefined;
    ConnectionState: ((connected: boolean) => void) | undefined;

    private connection: HubConnection;

    constructor(url: string) {
        this.connection = new HubConnectionBuilder()
            .withUrl(url, { withCredentials: false })
            .withAutomaticReconnect(new ReconnectRetryPolicy())
            .configureLogging(LogLevel.Information)
            .build();

        this.connection.on("receiveReport", (color, command) => this.ReceiveReport?.call(this, color, command));
        this.connection.on("ReceiveTheme", theme => console.log(theme));

        this.connection.onreconnecting(() => this.ConnectionState?.call(this, false));
        this.connection.onreconnected(() => this.ConnectionState?.call(this, true));
    }

    async start() {
        try {
            await this.connection.start();
            this.ConnectionState?.call(this, true);
        } catch (error) {
            console.log(`Signalr fail to connect. Attempt to reconnect after 1 minute`)
            this.ConnectionState?.call(this, false);
            setTimeout(async () => {
                await this.start();
            }, 1000 * 60);
        }
    }

    async stop() {
        if (this.connection.state !== HubConnectionState.Disconnected)
            await this.connection.stop();
        this.ConnectionState?.call(this, false);
    }

    sendCommand = (commandType: string) => this.connection.send('sendCommand', commandType);

}

/** Configures signalr.HubConnection to automatically attempt to reconnect if the connection is lost.
 *  ##### Retry count: 
 * * count < 10 - interval 1s
 * * count < 50 - interval 30s
 * * count > 50 - interval 1m
 */
class ReconnectRetryPolicy implements IRetryPolicy {
    nextRetryDelayInMilliseconds(retryContext: RetryContext): number | null {
        var count = retryContext.previousRetryCount;
        switch (true) {
            case count < 10: return 1000;
            case count < 50: return 1000 * 30;
            default: return 1000 * 60;
        };
    }
}