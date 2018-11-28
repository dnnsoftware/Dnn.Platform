import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import {
    languages as LanguagesActions,
    visiblePanel as VisiblePanelActions,
    languageEditor as LanguageEditorActions
} from "../../../actions";
import LanguageRow from "./languageRow";
import LanguageEditor from "./languageEditor";
import Collapse from "dnn-collapsible";
import "./style.less";
import { AddIcon } from "dnn-svg-icons";
import util from "../../../utils";
import resx from "../../../resources";

let tableFields = [];
let isHost = false;

class LanguagesPanel extends Component {
    constructor() {
        super();
        this.state = {
            languageList: [],
            openId: "",
            openMode: 1,
            contentLocalizationEnabled: false
        };
        isHost = util.settings.isHost;
    }

    loadData() {
        const {props} = this;
        if (props.languageList) {
            if (props.portalId === undefined || props.languageList.PortalId === props.portalId) {
                return false;
            }
            else {
                return true;
            }
        }
        else {
            return true;
        }
    }

    componentDidMount() {
        const {props} = this;

        this.getHeaderColumns(props.contentLocalizationEnabled);

        if (!this.loadData()) {
            this.setState({
                languageList: props.languageList,
                contentLocalizationEnabled: props.contentLocalizationEnabled
            });
            return;
        }
        props.dispatch(LanguagesActions.getLanguages(props.portalId, (data) => {
            this.setState({
                languageList: Object.assign({}, data.Languages),
                contentLocalizationEnabled: props.contentLocalizationEnabled
            });
        }));
    }

    componentDidUpdate(props) {
        let {state} = this;        

        if (state.contentLocalizationEnabled !== props.contentLocalizationEnabled) {
            props.dispatch(LanguagesActions.getLanguages(props.portalId, () => {
                this.getHeaderColumns(props.contentLocalizationEnabled);
                this.setState({
                    contentLocalizationEnabled: props.contentLocalizationEnabled
                });
            }));
        }
    }

    getHeaderColumns(contentLocalizationEnabled) {
        tableFields = [];
        if (contentLocalizationEnabled) {
            tableFields.push({ "name": resx.get("Culture.Header"), "id": "adv-Culture" });
            tableFields.push({ "name": resx.get("Pages.Header"), "id": "adv-Pages" });
            tableFields.push({ "name": resx.get("Translated.Header"), "id": "adv-Translated" });
            tableFields.push({ "name": resx.get("Active.Header"), "id": "adv-Active" });
            tableFields.push({ "name": resx.get("Enabled.Header"), "id": "adv-Enabled" });
            tableFields.push({ "name": "", "id": "adv-Actions" });
        }
        else {
            tableFields.push({ "name": resx.get("Culture.Header"), "id": "Culture" });
            tableFields.push({ "name": resx.get("Enabled.Header"), "id": "Enabled" });
        }
    }

    renderHeader() {
        let tableHeaders = tableFields.map((field) => {
            let className = "languages-items header-" + field.id;
            return <div className={className} key={"header-" + field.id}>
                <span>{field.name}&nbsp; </span>
            </div>;
        });
        return <div className="header-row">{tableHeaders}</div>;
    }

    uncollapse(id) {
        this.setState({
            openId: id
        });
    }

    collapse() {
        if (this.state.openId !== "") {
            this.setState({
                openId: ""
            });
        }
    }

    toggle(openId, mode) {
        const {props} = this;
        if (props.languageClientModified) {
            util.utilities.confirm(resx.get("SettingsRestoreWarning"), resx.get("Yes"), resx.get("No"), () => {
                props.dispatch(LanguagesActions.cancelLanguageClientModified());
                if (openId !== "") {
                    this.uncollapse(openId);
                    this.setState({
                        openMode: mode
                    });
                }
            });
        }
        else {
            if (openId !== "") {
                this.uncollapse(openId);
                this.setState({
                    openMode: mode
                });
            }
        }
    }

    onUpdateLanguage(languageDetail) {
        const {props} = this;
        if (languageDetail.LanguageId && languageDetail.LanguageId !== -1) {
            props.dispatch(LanguagesActions.updateLanguage(languageDetail, (data) => {
                util.utilities.notify(resx.get("LanguageUpdateSuccess"));
                this.collapse();
                if (data.RedirectUrl) {
                    window.parent.location = data.RedirectUrl;
                }
                else {
                    props.dispatch(LanguagesActions.getLanguages(props.portalId));
                }
            }, (error) => {
                const errorMessage = JSON.parse(error.responseText);
                util.utilities.notifyError(errorMessage.Message);
            }));
        }
        else {
            props.dispatch(LanguagesActions.addLanguage(languageDetail, () => {
                util.utilities.notify(resx.get("LanguageCreateSuccess"));
                this.collapse();
                props.dispatch(LanguagesActions.getLanguages(props.portalId));
                props.dispatch(LanguagesActions.getAllLanguages());
                props.dispatch(LanguagesActions.getLanguageSettings(props.portalId, props.cultureCode));
            }, (error) => {
                const errorMessage = JSON.parse(error.responseText);
                util.utilities.notifyError(errorMessage.Message);
            }));
        }
    }

    onUpdateLanguageRoles(roles) {
        const {props} = this;
        props.dispatch(LanguagesActions.updateLanguageRoles(roles, () => {
            util.utilities.notify(resx.get("LanguageUpdateSuccess"));
            this.collapse();
        }, (error) => {
            const errorMessage = JSON.parse(error.responseText);
            util.utilities.notifyError(errorMessage.Message);
        }));
    }

    onOpenEditor(language) {
        this.props.dispatch(VisiblePanelActions.selectPanel(3));
        this.props.dispatch(LanguageEditorActions.setLanguageBeingEdited(language));
    }

    onOpenPageList(language) {
        this.props.dispatch(VisiblePanelActions.selectPanel(6));
        this.props.dispatch(LanguageEditorActions.setLanguageBeingEdited(language));
    }

    onLocalizePages() {
        //
    }

    /* eslint-disable react/no-danger */
    renderedLanguages() {
        let i = 0;
        if (this.props.languageList) {
            return this.props.languageList.map((item, index) => {
                let id = "row-" + i++;
                return (
                    <LanguageRow
                        languageId={item.LanguageId}
                        name={this.props.languageDisplayMode === "NATIVE" ? item.NativeName : item.EnglishName}
                        code={item.Code}
                        icon={item.Icon}
                        enabled={item.Enabled}
                        localizablePages={item.LocalizablePages}
                        localizedStatus={item.LocalizedStatus}
                        translatedPages={item.TranslatedPages}
                        translatedStatus={item.TranslatedStatus}
                        active={item.Active}
                        isLocalized={item.IsLocalized}
                        contentLocalizationEnabled={this.props.contentLocalizationEnabled}
                        isDefault={item.IsDefault}
                        index={index}
                        key={"languageItem-" + index}
                        closeOnClick={true}
                        openId={this.state.openId}
                        OpenCollapse={this.toggle.bind(this)}
                        onOpenEditor={this.onOpenEditor.bind(this, item)}
                        onOpenPageList={this.onOpenPageList.bind(this, item)}
                        onLocalizePages={this.onLocalizePages.bind(this, item)}
                        Collapse={this.collapse.bind(this)}
                        id={id}>
                        <LanguageEditor
                            languageId={item.LanguageId}
                            code={item.Code}
                            languageDisplayMode={this.props.languageDisplayMode}
                            portalId={this.props.portalId}
                            Collapse={this.collapse.bind(this)}
                            onUpdate={this.onUpdateLanguage.bind(this)}
                            onUpdateRoles={this.onUpdateLanguageRoles.bind(this)}
                            id={id}
                            openId={this.state.openId}
                            openMode={this.state.openMode}
                        />
                    </LanguageRow>
                );
            });
        }
    }

    render() {
        let opened = (this.state.openId === "add");
        return (
            <div>
                <div className="language-items">
                    <div className="AddItemRow">
                        <div className="sectionTitle-languages">{resx.get("Languages")}</div>
                        {isHost &&
                            <div className={opened ? "AddItemBox-active" : "AddItemBox"} onClick={this.toggle.bind(this, opened ? "" : "add", 1)}>
                                <div className="add-icon" dangerouslySetInnerHTML={{ __html: AddIcon }}>
                                </div> {resx.get("cmdCreateLanguage")}
                            </div>
                        }
                    </div>
                    <div className="language-items-grid">
                        {this.renderHeader()}
                        <Collapse isOpened={opened} style={{ float: "left", width: "100%" }}>
                            <LanguageRow
                                name={"-"}
                                code={""}
                                enabled={false}
                                contentLocalizationEnabled={this.props.contentLocalizationEnabled}
                                isDefault={false}
                                index={"add"}
                                key={"languageItem-add"}
                                closeOnClick={true}
                                openId={this.state.openId}
                                OpenCollapse={this.toggle.bind(this)}
                                onOpenEditor={this.onOpenEditor.bind(this)}
                                onLocalizePages={this.onLocalizePages.bind(this)}
                                Collapse={this.collapse.bind(this)}
                                id={"add"}>
                                {opened && <LanguageEditor
                                    portalId={this.props.portalId}
                                    languageDisplayMode={this.props.languageDisplayMode}
                                    Collapse={this.collapse.bind(this)}
                                    onUpdate={this.onUpdateLanguage.bind(this)}
                                    id={"add"}
                                    openId={this.state.openId}
                                    openMode={this.state.openMode}/>
                                }
                            </LanguageRow>
                        </Collapse>
                        {this.renderedLanguages()}
                    </div>
                </div>

            </div >
        );
    }
}

LanguagesPanel.propTypes = {
    dispatch: PropTypes.func.isRequired,
    tabIndex: PropTypes.number,
    languageList: PropTypes.array,
    portalId: PropTypes.number,
    cultureCode: PropTypes.string,
    languageDisplayMode: PropTypes.string,
    languageClientModified: PropTypes.bool,
    contentLocalizationEnabled: PropTypes.bool
};

function mapStateToProps(state) {
    return {
        languageList: state.languages.languageList,
        tabIndex: state.pagination.tabIndex,
        languageClientModified: state.languages.languageClientModified
    };
}

export default connect(mapStateToProps)(LanguagesPanel);