import React, { Component, PropTypes } from "react";
import { connect } from "react-redux";
import {
    pagination as PaginationActions,
    siteSettings as SiteSettingsActions
} from "../../actions";
import SiteAliasRow from "./siteAliasRow";
import SiteAliasEditor from "./siteAliasEditor";
import Collapse from "react-collapse";
import Select from "dnn-select";
import "./style.less";
import { AddIcon } from "dnn-svg-icons";
import util from "../../utils";
import resx from "../../resources";

let tableFields = [];

class SiteAliasesPanel extends Component {
    constructor() {
        super();
        this.state = {
            siteAliases: [],
            openId: ""
        };
    }

    componentWillMount() {
        const {props} = this;

        if (props.siteAliases) {
            this.setState({
                siteAliases: props.siteAliases
            });
            return;
        }
        props.dispatch(SiteSettingsActions.getSiteAliases(props.portalId, (data) => {
            this.setState({
                siteAliases: Object.assign({}, data.PortalAliases)
            });
        }));
    }

    componentWillReceiveProps(props) {
        let {state} = this;
        if (tableFields.length === 0) {
            tableFields.push({ "name": resx.get("Alias.Header"), "id": "Alias" });
            tableFields.push({ "name": resx.get("Browser.Header"), "id": "Browser" });
            tableFields.push({ "name": resx.get("Theme.Header"), "id": "Theme" });
            if (props.languages.length > 1) {
                tableFields.push({ "name": resx.get("Language.Header"), "id": "Language" });
            }
            tableFields.push({ "name": resx.get("Primary.Header"), "id": "Primary" });
        }
    }

    renderHeader() {
        let tableHeaders = tableFields.map((field) => {
            let className = "alias-items header-" + field.id;
            return <div className={className} key={"header-" + field.id}>
                <span>{field.name}&nbsp; </span>
            </div>;
        });
        return <div className="header-row">{tableHeaders}</div>;
    }

    uncollapse(id) {
        setTimeout(() => {
            this.setState({
                openId: id
            });
        }, this.timeout);
    }

    collapse() {
        if (this.state.openId !== "") {
            this.setState({
                openId: ""
            });
        }
    }

    toggle(openId) {
        if (openId !== "") {
            this.uncollapse(openId);
        }
    }

    onUpdateSiteAlias(aliasDetail) {
        const {props, state} = this;
        if (aliasDetail.PortalAliasID) {
            props.dispatch(SiteSettingsActions.updateSiteAlias(aliasDetail, (data) => {
                util.utilities.notify(resx.get("SiteAliasUpdateSuccess"));
                this.collapse();
                props.dispatch(SiteSettingsActions.getSiteAliases(props.portalId));
            }, (error) => {
                const errorMessage = JSON.parse(error.responseText);
                util.utilities.notifyError(errorMessage.Message);
            }));
        }
        else {
            props.dispatch(SiteSettingsActions.addSiteAlias(aliasDetail, (data) => {
                util.utilities.notify(resx.get("SiteAliasCreateSuccess"));
                this.collapse();
                props.dispatch(SiteSettingsActions.getSiteAliases(props.portalId));
            }, (error) => {
                const errorMessage = JSON.parse(error.responseText);
                util.utilities.notifyError(errorMessage.Message);
            }));
        }
    }

    onDeleteSiteAlias(aliasId) {
        const {props, state} = this;
        util.utilities.confirm(resx.get("SiteAliasDeletedWarning"), resx.get("Yes"), resx.get("No"), () => {
            const itemList = props.siteAliases.filter((item) => item.PortalAliasID !== aliasId);
            props.dispatch(SiteSettingsActions.deleteSiteAlias(aliasId, itemList, () => {
                util.utilities.notify(resx.get("SiteAliasDeleteSuccess"));
                this.collapse();
            }, (error) => {
                util.utilities.notify(resx.get("SiteAliasDeleteError"));
            }));
        });
    }

    /* eslint-disable react/no-danger */
    renderedSiteAliases() {
        let i = 0;
        if (this.props.siteAliases) {
            return this.props.siteAliases.map((item, index) => {
                let id = "row-" + i++;
                return (
                    <SiteAliasRow
                        aliasId={item.PortalAliasID}
                        alias={item.HTTPAlias}
                        browser={item.BrowserType}
                        skin={item.Skin}
                        language={item.CultureCode}
                        isPrimary={item.IsPrimary}
                        deletable={item.Deletable}
                        editable={item.Editable}
                        showLanguageColumn={this.props.languages && this.props.languages.length > 1}
                        index={index}
                        key={"aliasItem-" + index}
                        closeOnClick={true}
                        openId={this.state.openId}
                        OpenCollapse={this.toggle.bind(this)}
                        Collapse={this.collapse.bind(this)}
                        onDelete={this.onDeleteSiteAlias.bind(this, item.PortalAliasID)}
                        id={id}>
                        <SiteAliasEditor
                            aliasId={item.PortalAliasID}
                            Collapse={this.collapse.bind(this)}
                            onUpdate={this.onUpdateSiteAlias.bind(this)}
                            id={id}
                            openId={this.state.openId} />
                    </SiteAliasRow>
                );
            });
        }
    }

    render() {
        let opened = (this.state.openId === "add");
        return (
            <div>
                <div className="alias-items">
                    <div className="AddItemRow">
                        <div className="sectionTitle">{resx.get("SiteAliases")}</div>
                        <div className={opened ? "AddItemBox-active" : "AddItemBox"} onClick={this.toggle.bind(this, opened ? "" : "add")}>
                            <div className="add-icon" dangerouslySetInnerHTML={{ __html: AddIcon }}>
                            </div> Add Alias
                        </div>
                    </div>
                    <div className="alias-items-grid">
                        {this.renderHeader()}
                        <Collapse isOpened={opened} style={{ float: "left", width: "100%" }}>
                            <SiteAliasRow
                                alias={"-"}
                                browser={"-"}
                                skin={"-"}
                                language={"-"}
                                isPrimary={"-"}
                                deletable={false}
                                editable={false}
                                showLanguageColumn={this.props.languages && this.props.languages.length > 1}
                                index={"add"}
                                key={"aliasItem-add"}
                                closeOnClick={true}
                                openId={this.state.openId}
                                OpenCollapse={this.toggle.bind(this)}
                                Collapse={this.collapse.bind(this)}
                                onDelete={this.onDeleteSiteAlias.bind(this)}
                                id={"add"}>
                                <SiteAliasEditor
                                    Collapse={this.collapse.bind(this)}
                                    onUpdate={this.onUpdateSiteAlias.bind(this)}
                                    id={"add"}
                                    openId={this.state.openId} />
                            </SiteAliasRow>
                        </Collapse>
                        {this.renderedSiteAliases()}
                    </div>
                </div>

            </div >
        );
    }
}

SiteAliasesPanel.propTypes = {
    dispatch: PropTypes.func.isRequired,
    tabIndex: PropTypes.number,
    siteAliases: PropTypes.array,
    browsers: PropTypes.array,
    languages: PropTypes.array,
    skins: PropTypes.array,
    portalId: PropTypes.number
};

function mapStateToProps(state) {
    return {
        siteAliases: state.siteSettings.siteAliases,
        browsers: state.siteSettings.browsers,
        languages: state.siteSettings.languages,
        skins: state.siteSettings.skins,
        tabIndex: state.pagination.tabIndex
    };
}

export default connect(mapStateToProps)(SiteAliasesPanel);