import React, {Component} from "react";
import PropTypes from "prop-types";
import Localization from "../../../localization";
import { Collapsible, GridCell, Button, RadioButtons, Label, Dropdown, SingleLineInputWithError } from "@dnnsoftware/dnn-react-common";

const portalAliasUsageType = {
    Default: 0,
    ChildPagesInherit: 1,
    ChildPagesDoNotInherit: 2,
    InheritedFromParent: 3
};

class EditUrl extends Component {
    constructor() {
        super();
        this.state = {
            hasChanges: false
        };
    }

    onChangeField(key, event) {
        const {onChange} = this.props;
        let value = event;
        if (event.value) {
            value = event.value;
        }  else if (event.target) {
            value = event.target.value;
        }
        
        onChange(key, value);

        this.setState({
            hasChanges: true
        });
    }
    
    getUrlTypeOptions() {
        return [
            {
                label: "Active (200)",
                value: 200
            },
            {
                label: "Redirect (301)",
                value: 301
            }
        ];
    }
    
    getOptions(siteAliases) {
        return siteAliases.map(alias => {
            return {
                label: alias.Value,
                value: alias.Key
            };
        });
    }
    
    getSiteAliasUsageOptions(hasParent) {
        let options = [
            {value: portalAliasUsageType.ChildPagesDoNotInherit, label: Localization.get("Pages_Seo_SelectedAliasUsageOptionThisPageOnly")}, 
            {value: portalAliasUsageType.ChildPagesInherit, label: Localization.get("Pages_Seo_SelectedAliasUsageOptionPageAndChildPages")}
        ];
                       
        if (hasParent) {
            options.push({
                value: portalAliasUsageType.InheritedFromParent, label: Localization.get("Pages_Seo_SelectedAliasUsageOptionSameAsParent")
            });
        }
        
        return options;
    }
    
    render() {
        const {url, saving, pageHasParent, siteAliases, primaryAliasId, isOpened, onSave, onCancel} = this.props;
        const aliases = this.getOptions(siteAliases);
        const siteAliasUsageOptions = this.getSiteAliasUsageOptions(pageHasParent);
        return (
            <Collapsible accordion={true} isOpened={isOpened} keepCollapsedContent={true} className={"editUrl"}>
                <div>
                    <GridCell>
                        <GridCell columnSize={50} className="left-column">
                            <Label
                                labelType="block"
                                tooltipMessage={Localization.get("Pages_Seo_SiteAlias.Help")}
                                label={Localization.get("Pages_Seo_SiteAlias")} />
                            <Dropdown options={aliases}
                                value={url.siteAlias.Key} 
                                onSelect={this.onChangeField.bind(this, "siteAlias")} 
                                withBorder={true} />
                        </GridCell>
                        <GridCell columnSize={50} className="right-column">
                            <SingleLineInputWithError
                                style={{width: "100%"}}
                                label={Localization.get("Pages_Seo_UrlPath")}
                                tooltipMessage={Localization.get("Pages_Seo_UrlPath.Help")}
                                value={url.path} 
                                onChange={this.onChangeField.bind(this, "path")} />
                        </GridCell>
                    </GridCell>
                    {url.siteAlias.Key !== primaryAliasId &&
                    <GridCell>
                        <GridCell columnSize={100}>
                            <Label
                                labelType="block"
                                tooltipMessage={Localization.get("Pages_Seo_SelectedAliasUsage.Help")}
                                label={Localization.get("Pages_Seo_SelectedAliasUsage")} />
                            <RadioButtons
                                    options={siteAliasUsageOptions} 
                                    onChange={this.onChangeField.bind(this, "siteAliasUsage")}
                                    value={url.siteAliasUsage}/>                        
                        </GridCell>
                    </GridCell>}
                    <GridCell>
                        <GridCell columnSize={50} className="left-column">
                            <Label
                                labelType="block"
                                tooltipMessage={Localization.get("Pages_Seo_UrlType.Help")}
                                label={Localization.get("Pages_Seo_UrlType")} />
                            <Dropdown options={this.getUrlTypeOptions()}
                                value={url.statusCode.Key} 
                                onSelect={this.onChangeField.bind(this, "statusCode")} 
                                withBorder={true} />
                        </GridCell>
                        <GridCell columnSize={50} className="right-column">
                            {url.statusCode.Key === 301 && 
                            <SingleLineInputWithError
                                style={{width: "100%"}}
                                label={Localization.get("Pages_Seo_QueryString")}
                                tooltipMessage={Localization.get("Pages_Seo_QueryString.Help")}
                                value={url.queryString} 
                                onChange={this.onChangeField.bind(this, "queryString")} />}
                        </GridCell>
                    </GridCell>
                    <div className="buttons-box" style={{float: "left", margin: "0 0 20px 0"}}>
                        <Button type="secondary" onClick={onCancel} disabled={saving}>
                            {Localization.get("Cancel")}
                        </Button>
                        <Button type="primary" onClick={onSave} disabled={!this.state.hasChanges || saving}>
                            {Localization.get("Save")}
                        </Button>
                    </div>
                    <div style={{clear: "both"}}></div>
                </div>
            </Collapsible>
        );
    }
}

EditUrl.propTypes = {
    url: PropTypes.object.isRequired,
    siteAliases: PropTypes.arrayOf(PropTypes.object).isRequired,
    primaryAliasId: PropTypes.number,
    onChange: PropTypes.func.isRequired,
    isOpened: PropTypes.bool,
    pageHasParent: PropTypes.bool,
    className: PropTypes.string, 
    onSave: PropTypes.func.isRequired,
    onCancel: PropTypes.func.isRequired,
    saving: PropTypes.bool
};

export default EditUrl;