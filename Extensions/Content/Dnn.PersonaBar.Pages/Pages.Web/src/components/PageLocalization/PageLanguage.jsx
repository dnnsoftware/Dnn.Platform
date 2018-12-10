import React, {Component} from "react";
import PropTypes from "prop-types";
import Localization from "localization";
import Module from "./Module";
import { Checkbox, SvgIcons } from "@dnnsoftware/dnn-react-common";
import "./PageLanguage.less";
import utils from "../../utils";

class PageLanguage extends Component {

    constructor() {
        super();
        this.state = {
            allModulesLinked: false,
            allModulesSelected: false
        };
    }

    updateAllModules(key, value) {
        const {modules} = this.props;
        const CultureCode = this.props.local.CultureCode;
        modules.forEach((module, index) => {
            if (module.IsShared && key === "IsLocalized") {
                return;
            }
            this.props.onUpdateModules(CultureCode, index, value, key);
        });
    }

    selectAllModules(e) {
        const allModulesSelected = e;
        this.setState({ allModulesSelected }, ()=> {
            this.updateAllModules("IsTranslated", e);
        });
    }

    linkAllModules() {
        const allModulesLinked = !this.state.allModulesLinked;
        this.setState({ allModulesLinked }, () => {
            this.updateAllModules("IsLocalized", allModulesLinked);
        });
    } 

    onUpdatePages(key, e) {
        const value = e.target ? e.target.value: e;
        const CultureCode = this.props.local.CultureCode;
        this.props.onUpdatePages(CultureCode, key, value);
    }

    onViewPage(url, e){
        utils.getUtilities().closePersonaBar(function () {
            window.parent.location = url;
        });

        e.preventDefault();
        e.stopPropagation();
    }

    /* eslint-disable react/no-danger */
    render() {
        const iconSrc = this.props.local && this.props.local.Icon ? this.props.local.Icon : "";
        const cultureCode = this.props.local && this.props.local.CultureCode ? this.props.local.CultureCode : "";
        const page = this.props.page || {};
        const modules = this.props.modules || [];
        const moduleComponents = modules.map((module, index) => {
            return <Module
                key={module.id}
                isDefault={this.props.isDefault}
                module={module}
                onUpdateModules={this.props.onUpdateModules}
                onDeleteModule={this.props.onDeleteModule}
                onRestoreModule={this.props.onRestoreModule}
                index={index}
                cultureCode={cultureCode}
                />;
        });
        return (
            <div className="page-language">
                <div className="page-language-row">
                    <img src={iconSrc} alt={cultureCode} />
                    <span>{cultureCode}</span>
                    <a className="icon" href={page.PageUrl} onClick={this.onViewPage.bind(this, page.PageUrl)} dangerouslySetInnerHTML={{ __html: SvgIcons.EyeIcon }} aria-label="View"></a>
                </div>
                <div className="page-language-row">
                    <input type="text" value={page.TabName} onChange={this.onUpdatePages.bind(this, "TabName") } aria-label="Name" />
                    <input type="text" value={page.Title} onChange={this.onUpdatePages.bind(this, "Title") } aria-label="Title" />
                    <textarea value={page.Description} onChange={this.onUpdatePages.bind(this, "Description") } aria-label="Description"/>
                </div>
                <div className="page-language-row">
                    <div className="page-language-row-header">
                        <span className="icon" dangerouslySetInnerHTML={{ __html: SvgIcons.ModuleIcon }} />
                        <span>{Localization.get("ModulesOnThisPage") }</span>
                        {!this.props.isDefault && <div className="icons-container">
                            <span
                            className={`icon float-left ${(this.state.allModulesLinked ? " blue" : "")}`}
                            onClick={this.linkAllModules.bind(this)  }
                            dangerouslySetInnerHTML={{ __html: SvgIcons.LinkIcon }} />
                            <Checkbox
                            style={{ float: "left" }}
                            value={this.state.allModulesSelected}
                            onChange={this.selectAllModules.bind(this) } />
                        </div>}
                    </div>
                    {moduleComponents }
                    <div className="module-row footer">
                        <Checkbox
                            style={{ float: "right" }}
                            value={page.IsTranslated}
                            onChange={this.onUpdatePages.bind(this, "IsTranslated") } />
                        <div>{Localization.get("TranslatedCheckbox") }</div>
                    </div>
                    <div className="module-row footer">
                        <Checkbox
                            style={{ float: "right" }}
                            value={page.IsPublished}
                            onChange={this.onUpdatePages.bind(this, "IsPublished") } />
                        <div>{Localization.get("PublishedCheckbox") }</div>
                    </div>
                </div>
            </div>
        );
    }
}

PageLanguage.propTypes = {
    local: PropTypes.object.isRequired,
    page: PropTypes.object.isRequired,
    modules: PropTypes.object.isRequired,
    onUpdatePages: PropTypes.func,
    onUpdateModules: PropTypes.func,
    onDeleteModule: PropTypes.func,
    onRestoreModule: PropTypes.func,
    isDefault: PropTypes.bool
};

export default PageLanguage;