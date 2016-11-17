import React, {Component, PropTypes} from "react";
import Localization from "localization";
import Tooltip from "dnn-tooltip";
import "./PageLanguage.less";

import { EyeIcon, SettingsIcon, ModuleIcon } from "dnn-svg-icons";

class PageLanguage extends Component {

    onOpenSettings() {

    }

    viewPage() {

    }

    renderModules() {
        const {modules} = this.props;
        if (!modules) {
            return false;
        }
        return modules.map((module) => {
            return <div className="module-row">
                <Tooltip
                    messages={[module.ModuleTitle]}
                    style={{ float: "left", position: "static" }}
                />
                <input type="text" value={module.ModuleTitle} />
            </div>;
        });
    }
    /* eslint-disable react/no-danger */
    render() {
        const iconSrc =  this.props.local && this.props.local.Icon ? this.props.local.Icon : "";
        const cultureCode = this.props.local && this.props.local.CultureCode ? this.props.local.CultureCode : "";
        const page = this.props.page ? this.props.page: {}; 
        return (
            <div className="page-language">
                <div className="page-language-row">
                    <img src={iconSrc} />
                    <span>{cultureCode}</span>
                    <div className="icon" dangerouslySetInnerHTML={{ __html: SettingsIcon }} onClick={this.onOpenSettings.bind(this) }></div>
                    <a className="icon" dangerouslySetInnerHTML={{ __html: EyeIcon }}></a>
                </div>
                <div className="page-language-row">
                    <input type="text" value={page.TabName} />
                    <input type="text" value={page.Title} />
                    <textarea value={page.Title} />
                </div>
                <div className="page-language-row">
                    <div className="page-language-row-header">
                        <span className="icon" dangerouslySetInnerHTML={{ __html: ModuleIcon }} />
                        <span>{Localization.get("ModulesOnThisPage") }</span>
                    </div>
                    {this.renderModules() }
                </div>
            </div>
        );
    }
}

PageLanguage.propTypes = {
    local: PropTypes.object.isRequired,
    page: PropTypes.object.isRequired,
    modules: PropTypes.object.isRequired
};

export default PageLanguage;


        //  data: {
        //         Locals: [
        //             {CultureCode: "Da-da", Icon: "/images/Flags/da-DK.gif"}
        //         ],
        //         Modules: [
        //             {ModuleTitle: "Blu-Ray Players"}
        //         ],
        //         Pages: [
        //            {Description: "", Title: "", TabName: "Home"} 
        //         ]
        //     }