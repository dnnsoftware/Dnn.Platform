import React, { Component } from "react";
import PropTypes from "prop-types";
import Collapse from "dnn-collapsible";
import "./style.less";
import { CheckMarkIcon, SettingsIcon, UsersIcon, LanguagesIcon, LanguagesPageIcon } from "dnn-svg-icons";
import Flag from "dnn-flag";

class LanguageRow extends Component {
    componentDidMount() {
        let opened = (this.props.openId !== "" && this.props.id === this.props.openId);
        this.setState({
            opened
        });
    }

    toggle(mode) {
        if ((this.props.openId !== "" && this.props.id === this.props.openId)) {
            if (mode !== this.state.openedMode) {
                this.props.OpenCollapse(this.props.id, mode);
                this.setState({
                    openedMode: mode
                });
            }
        }
        else {
            this.props.OpenCollapse(this.props.id, mode);
            this.setState({
                openedMode: mode
            });
        }
    }

    getLanguageNameDisplay(name, code, isDefault) {
        if (this.props.id !== "add") {
            return (
                <div>
                    <div className="language-flag"><Flag culture={code} title={name} /></div>
                    <div className="language-name">{isDefault ? name + " **" : name}</div>
                </div>);
        }
        else return <span>-</span>;
    }

    /* eslint-disable react/no-danger */
    getActiveDisplay(prop) {
        if (this.props.id !== "add") {
            if (prop && this.props.isLocalized) {
                return <div className="checkMarkIcon" dangerouslySetInnerHTML={{ __html: CheckMarkIcon }}></div>;
            }
            else return <span>&nbsp; </span>;
        }
        else return <span>-</span>;
    }

    getEnabledDisplay(prop) {
        if (this.props.id !== "add") {
            if (prop) {
                return <div className="checkMarkIcon" dangerouslySetInnerHTML={{ __html: CheckMarkIcon }}></div>;
            }
            else return <span>&nbsp; </span>;
        }
        else return <span>-</span>;
    }

    getPagesDisplay(pages, status) {
        if (this.props.id !== "add") {
            if (pages && this.props.isLocalized) {
                return this.props.isLocalized ? <div className="pages-number-status">
                    <div className="pages-number">{pages}</div>
                    <div className="pages-status">{status ? "(" + status + ")" : ""}</div>
                </div> : "";
            }
            else if (this.props.isDefault) {
                return <div className="pages-number-status">
                    <div className="pages-number">{pages}</div>
                    <div className="pages-status">{status ? "(" + status + ")" : ""}</div>
                </div>;
            }
            else return <span>&nbsp; </span>;
        }
        else return <span>-</span>;
    }

    getTranslatorBtnClassName() {
        const {props, state} = this;
        let name = "translator-icon";
        if (props.openId !== "" && props.id === this.props.openId) {
            if (props.openId !== "add") {
                if (state.openedMode === 1) {
                    name = "translator-icon";
                }
                else {
                    name = "translator-icon-active";
                }
            }
            else {
                name = "translator-icon-hidden";
            }
        }
        return name;
    }

    getEditBtnClassName() {
        const {props, state} = this;
        let name = "edit-icon";
        if (this.props.openId !== "" && this.props.id === this.props.openId) {
            if (props.openId !== "add") {
                if (state.openedMode === 2) {
                    name = "edit-icon";
                }
                else {
                    name = "edit-icon-active";
                }
            }
            else {
                name = "edit-icon-hidden";
            }
        }
        return name;
    }

    getEditorBtnClassName() {
        const {props} = this;
        let name = "editor-icon";
        if (this.props.openId !== "" && this.props.id === this.props.openId) {
            if (props.openId !== "add") {
                name = "editor-icon";
            }
            else {
                name = "editor-icon-hidden";
            }
        }
        return name;
    }

    getPageEditorBtnClassName() {
        const {props} = this;
        let name = "page-editor-icon";
        if (this.props.openId !== "" && this.props.id === this.props.openId) {
            if (props.openId !== "add") {
                name = "page-editor-icon";
            }
            else {
                name = "page-editor-icon-hidden";
            }
        }
        return name;
    }
    /* eslint-disable react/no-danger */
    render() {
        const {props} = this;
        const isAddMode = props.openId === "add";

        let opened = (this.props.openId !== "" && this.props.id === this.props.openId);
        if (props.contentLocalizationEnabled) {
            return (
                <div className={"collapsible-component-language" + (opened ? " row-opened" : "")}>
                    <div className={"collapsible-header-language " + !opened} >
                        <div className={"row"}>
                            <div className="language-item item-row-name-adv">
                                {this.getLanguageNameDisplay(props.name, props.code, props.isDefault)}
                            </div>
                            <div className="language-item item-row-pages">
                                {this.getPagesDisplay(props.localizablePages, props.localizedStatus)}&nbsp;
                            </div>
                            <div className="language-item item-row-translated">
                                {this.getPagesDisplay(props.translatedPages, props.translatedStatus)}&nbsp;
                            </div>
                            <div className="language-item item-row-active">
                                {this.getActiveDisplay(props.active)}
                            </div>
                            <div className="language-item item-row-enabled-adv">
                                {this.getEnabledDisplay(props.enabled)}
                            </div>
                            <div className="language-item item-row-actionButtons item-row-actionButtons-adv">
                                {!props.isDefault && <div className={this.getPageEditorBtnClassName()} dangerouslySetInnerHTML={{ __html: LanguagesPageIcon }} onClick={props.onOpenPageList}></div>}
                                <div className={this.getEditorBtnClassName()} dangerouslySetInnerHTML={{ __html: LanguagesIcon }} onClick={props.onOpenEditor}></div>
                                {!props.isDefault &&
                                    <div className={this.getTranslatorBtnClassName()} dangerouslySetInnerHTML={{ __html: UsersIcon }} onClick={this.toggle.bind(this, 2)}></div>
                                }
                                <div className={this.getEditBtnClassName()} dangerouslySetInnerHTML={{ __html: SettingsIcon }} onClick={this.toggle.bind(this, 1)}></div>
                            </div>
                        </div>
                    </div>

                    {isAddMode && <Collapse autoScroll={true} isOpened={opened} style={{ float: "left", width: "100%" }}>{opened && props.children}</Collapse>}
                    {!isAddMode && <Collapse autoScroll={true} className="language-permission-grid" isOpened={opened} style={{ float: "left" }}>{opened && props.children}</Collapse>}
                </div>
            );
        }
        else {
            return (
                <div className={"collapsible-component-language" + (opened ? " row-opened" : "")}>
                    <div className={"collapsible-header-language " + !opened} >
                        <div className={"row"}>
                            <div className="language-item item-row-name">
                                {this.getLanguageNameDisplay(props.name, props.code, props.isDefault)}
                            </div>
                            <div className="language-item item-row-enabled">
                                {this.getEnabledDisplay(props.enabled)}
                            </div>
                            <div className="language-item item-row-actionButtons">
                                <div className={this.getEditorBtnClassName()} dangerouslySetInnerHTML={{ __html: LanguagesIcon }} onClick={props.onOpenEditor}></div>
                                {!props.isDefault &&
                                    <div className={this.getTranslatorBtnClassName()} dangerouslySetInnerHTML={{ __html: UsersIcon }} onClick={this.toggle.bind(this, 2)}></div>
                                }
                                <div className={this.getEditBtnClassName()} dangerouslySetInnerHTML={{ __html: SettingsIcon }} onClick={this.toggle.bind(this, 1)}></div>
                            </div>
                        </div>
                    </div>
                    {isAddMode && <Collapse autoScroll={true} isOpened={opened} style={{ float: "left", width: "100%" }}>{opened && props.children}</Collapse>}
                    {!isAddMode && <Collapse autoScroll={true} className="language-permission-grid" isOpened={opened} style={{ float: "left" }}>{opened && props.children}</Collapse>}
                </div>
            );
        }
    }
}

LanguageRow.propTypes = {
    languageId: PropTypes.number,
    name: PropTypes.string,
    code: PropTypes.string,
    icon: PropTypes.string,
    enabled: PropTypes.bool,
    localizablePages: PropTypes.number,
    localizedStatus: PropTypes.string,
    translatedPages: PropTypes.number,
    translatedStatus: PropTypes.string,
    active: PropTypes.bool,
    isLocalized: PropTypes.bool,
    contentLocalizationEnabled: PropTypes.bool,
    isDefault: PropTypes.bool,
    OpenCollapse: PropTypes.func,
    Collapse: PropTypes.func,
    id: PropTypes.string,
    openId: PropTypes.string,
    onOpenEditor: PropTypes.func,
    onOpenPageList: PropTypes.func,
    onLocalizePages: PropTypes.func
};

LanguageRow.defaultProps = {
    collapsed: true
};
export default (LanguageRow);
