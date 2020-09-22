import { MessageService } from "./MessageService"

export class ReactLoader {
    constructor(mService) {
        this.createBy = "SETemplateBuilderPlugin"
        if(mService){
            this.mService = mService;
        }else{
            window.icnReactService = new MessageService();
            this.mService = icnReactService
        }
        this.renderers = {}

        this.mService.getMessage().subscribe(data => {
            let message = data.payload.message;
            console.log("Message: " + message);
            const renderer = this.renderers[message];
            if (renderer) {
                renderer.call(this, data.payload);
            }
        })
    }

    //Register render function for message hanlder
    registerRender = (message, renderer,context) => {
        if(this.renderers[message]){
            console.log("Message handler is already registered. Please confirm the registration is on purpose.")
        }
        this.renderers[message] = renderer
    }
}
