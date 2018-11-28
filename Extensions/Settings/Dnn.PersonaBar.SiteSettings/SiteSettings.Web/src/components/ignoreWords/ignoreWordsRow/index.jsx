import React, { Component } from "react";
import PropTypes from "prop-types";
import Collapse from "dnn-collapsible";
import "./style.less";
import { EditIcon, TrashIcon } from "dnn-svg-icons";

class IgnoreWordsRow extends Component {
    componentDidMount() {
        let opened = (this.props.openId !== "" && this.props.id === this.props.openId);
        this.setState({
            opened
        });
    }

    toggle() {
        if ((this.props.openId !== "" && this.props.id === this.props.openId)) {
            //this.props.Collapse();
        }
        else {
            this.props.OpenCollapse(this.props.id);
        }
    }

    /* eslint-disable react/no-danger */
    render() {
        const {props} = this;
        let opened = (this.props.openId !== "" && this.props.id === this.props.openId);
        return (
            <div className={"collapsible-component-ignore-words" + (opened ? " row-opened" : "")}>
                {props.visible &&
                    <div className={"collapsible-header-ignore-words " + !opened} >
                        <div className={"row"}>
                            <div className="words-item item-row-tags">
                                {props.tags}</div>
                            <div className="words-item item-row-editButton">
                                <div className={opened ? "delete-icon-hidden" : "delete-icon"} dangerouslySetInnerHTML={{ __html: TrashIcon }} onClick={props.onDelete.bind(this)}></div>
                                <div className={opened ? "edit-icon-active" : "edit-icon"} dangerouslySetInnerHTML={{ __html: EditIcon }} onClick={this.toggle.bind(this)}></div>
                            </div>
                        </div>
                    </div>
                }
                <Collapse className="words-editor-wrapper" isOpened={opened} style={{ float: "left", width: "100%" }}>{opened && props.children}</Collapse>
            </div>
        );
    }
}

IgnoreWordsRow.propTypes = {
    wordsId: PropTypes.number,
    tags: PropTypes.string,
    OpenCollapse: PropTypes.func,
    Collapse: PropTypes.func,
    onDelete: PropTypes.func,
    id: PropTypes.string,
    openId: PropTypes.string,
    visible: PropTypes.bool
};

IgnoreWordsRow.defaultProps = {
    collapsed: true
};
export default (IgnoreWordsRow);
