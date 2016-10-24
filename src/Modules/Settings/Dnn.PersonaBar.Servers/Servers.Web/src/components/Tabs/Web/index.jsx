import React, {Component } from "react";
import GridSystem from "dnn-grid-system";
import InfoBlock from "../../common/InfoBlock";

import "../style.less";

import Localization from "../../../localization";

export default class Web extends Component {
    render() {
        return <div className="dnn-servers-info-tab">
            <GridSystem>
                <div>
                    <InfoBlock label={Localization.get("ServerInfo_OSVersion")} 
                        tooltip={Localization.get("ServerInfo_OSVersion.Help")} 
                        text={"Place holder"} />   

                    <InfoBlock label={Localization.get("ServerInfo_IISVersion")} 
                        tooltip={Localization.get("ServerInfo_IISVersion.Help")} 
                        text={"Place holder"} /> 

                    <InfoBlock label={Localization.get("ServerInfo_Framework")} 
                        tooltip={Localization.get("ServerInfo_Framework.Help")} 
                        text={"Place holder"} /> 

                    <div className="tooltipAdjustment">
                        <InfoBlock label={Localization.get("ServerInfo_Identity")} 
                            tooltip={Localization.get("ServerInfo_Identity.Help")} 
                            text={"Place holder"} />
                    </div> 

                    <InfoBlock label={Localization.get("ServerInfo_HostName")} 
                        tooltip={Localization.get("ServerInfo_HostName.Help")} 
                        text={"Place holder"} /> 
                </div>
                <div>
                    <InfoBlock label={Localization.get("ServerInfo_PhysicalPath")} 
                        tooltip={Localization.get("ServerInfo_PhysicalPath.Help")} 
                        text={"Place holder"} /> 

                    <InfoBlock label={Localization.get("ServerInfo_Url")} 
                        tooltip={Localization.get("ServerInfo_Url.Help")} 
                        text={"Place holder"} /> 

                    <InfoBlock label={Localization.get("ServerInfo_RelativePath")} 
                        tooltip={Localization.get("ServerInfo_RelativePath.Help")} 
                        text={"Place holder"} /> 

                    <InfoBlock label={Localization.get("ServerInfo_ServerTime")} 
                        tooltip={Localization.get("ServerInfo_ServerTime.Help")} 
                        text={"Place holder"} />
                </div>
            </GridSystem>
        </div>;
    }
}