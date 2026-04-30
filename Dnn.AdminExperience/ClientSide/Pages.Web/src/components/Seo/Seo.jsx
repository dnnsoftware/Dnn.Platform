import React, {Component} from "react";
import PropTypes from "prop-types";
import styles from "./style.module.less";
import { GridCell, Label, Dropdown, Switch } from "@dnnsoftware/dnn-react-common";
import Localization from "../../localization";
import PageUrls from "./PageUrls/PageUrls";
import PageHeaderTags from "../PageHeaderTags/PageHeaderTags";

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
            const v = i.toFixed(1) / 1;
            options.push({ value: v, label: v });
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

                <GridCell className="seo-row">
                    <PageHeaderTags
                        label={Localization.get("PageHeaderTags") }
                        value={page.pageHeaderTags}
                        onChange={(value) => onChangeField("pageHeaderTags", value)} />
                </GridCell>

                <GridCell className="seo-row new-section">
                    <Label
                        labelType="block"
                        tooltipMessage={Localization.get("SitemapPriority_tooltip") }
                        label={Localization.get("SitemapPriority") } />
                    <Dropdown options={this.getSitemapPriorityOptions() }
                        value={page.sitemapPriority}
                        onSelect={this.onSitemapPrioritySelected.bind(this) }
                        withBorder={true} />
                </GridCell>

                <GridCell className="seo-row new-section">
                    <Label
                        labelType="inline"
                        tooltipMessage={Localization.get("AllowIndexing_tooltip") }
                        label={Localization.get("AllowIndexing") }
                    />
                    <Switch
                        labelHidden={false}
                        onText={Localization.get("On") }
                        offText={Localization.get("Off") }
                        value={page.allowIndex}
                        onChange={onChangeField.bind(this, "allowIndex") } />
                </GridCell>
            </div>
        );
    }
}

Seo.propTypes = {
    page: PropTypes.object.isRequired,
    onChangeField: PropTypes.func.isRequired
};


export default Seo;
