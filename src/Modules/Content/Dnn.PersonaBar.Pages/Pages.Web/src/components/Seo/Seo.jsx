import React, {Component, PropTypes} from "react";
import styles from "./style.less";
import GridSystem from "dnn-grid-system";
import GridCell from "dnn-grid-cell";
import Label from "dnn-label";
import Dropdown from "dnn-dropdown";
import Localization from "../../localization";
import PageUrls from "./PageUrls/PageUrls";
import MultiLineInputWithError from "dnn-multi-line-input-with-error";
import Switch from "dnn-switch";

class Seo extends Component {

    onChangeField(key, event) {
        const {onChangeField} = this.props;
        onChangeField(key, event.target.value);
    }

    onSitemapPrioritySelected(option) {
        const {onChangeField} = this.props;
        onChangeField("sitemapPriority", option.value);
    }

    getSitemapPriorityOptions() {
        const options = [];
        for (let i = 0; i <= 1; i += 0.1) {
            const v = i.toFixed(1)/1;
            options.push({value: v, label: v});
        } 

        return options;
    }
    
    render() {
        const {page, onChangeField} = this.props;
        
        const editing = page.tabId !== 0;
        
        return (
            <div className={styles.seoContainer}>
                {editing &&
                <PageUrls pageUrls={page.pageUrls} 
                    siteAliases={page.siteAliases} 
                    pageHasParent={page.hasParent}
                    primaryAliasId={page.primaryAliasId} />} 
            
                <GridSystem>
                    <GridCell className="left-column">
                        <MultiLineInputWithError
                            label={Localization.get("PageHeaderTags")}
                            tooltipMessage={Localization.get("PageHeaderTags_tooltip")} 
                            value={page.pageHeadText}
                            onChange={this.onChangeField.bind(this, "pageHeadText")} />
                    </GridCell>
                    <GridCell className="right-column">
                        <GridCell>
                            <Label
                                labelType="block"
                                tooltipMessage={Localization.get("SitemapPriority_tooltip")}
                                label={Localization.get("SitemapPriority")} />
                            <Dropdown options={this.getSitemapPriorityOptions()}
                                value={page.sitemapPriority} 
                                onSelect={this.onSitemapPrioritySelected.bind(this)} 
                                withBorder={true} />
                        </GridCell>
                        <GridCell className="new-section">
                            <Label
                                labelType="inline"
                                tooltipMessage={Localization.get("AllowIndexing_tooltip")}
                                label={Localization.get("AllowIndexing")}
                                />
                            <Switch
                                labelHidden={true}
                                value={page.allowIndex}
                                onChange={onChangeField.bind(this, "allowIndex")} />
                    </GridCell>
                    </GridCell>
                </GridSystem>
            </div>
        );
    }
}

Seo.propTypes = {
    page: PropTypes.object.isRequired,
    onChangeField: PropTypes.func.isRequired
};


export default Seo;