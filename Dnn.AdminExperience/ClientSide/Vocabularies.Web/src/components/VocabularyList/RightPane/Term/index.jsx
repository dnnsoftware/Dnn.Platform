import React, { Component  } from "react";
import PropTypes from "prop-types";
import Collapsible from "react-collapse";
import { SvgIcons } from "@dnnsoftware/dnn-react-common";
import util from "utils";
import styles from "./style.module.less";


class Term extends Component {
    constructor() {
        super();
        this.state = {
            isOpened: false
        };
    }

    toggleTerm(event) {
        if (event) {
            event.preventDefault();
        }
        this.setState({
            isOpened: !this.state.isOpened
        });
    }

    onLiClick(event) {
        event.preventDefault();
        const {props} = this;
        if (props.isEditable) {
            this.toggleTerm();
        } else {
            this.onClick();
        }
    }

    UNSAFE_componentWillReceiveProps(props) {
        if (props.closeAll) {
            this.setState({
                isOpened: false
            });
        }
    }

    onClick() {
        const {props} = this;
        props.onClick(props.term);
    }

    render() {
        const {props, state} = this;
        const className = styles.termLi + (props.children.length > 0 ? " has-children" : "") + (state.isOpened ? " opened" : "") + (props.term.selected ? " selected" : "");

         
        return (
            <li className={className}>
                <div>
                    {props.children.length > 0 && !state.isOpened &&
                        <div className="edit-svg" onClick={this.toggleTerm.bind(this)}><SvgIcons.ArrowRightIcon /></div>
                    }
                    {props.children.length > 0 && state.isOpened &&
                        <div className="edit-svg" onClick={this.toggleTerm.bind(this)}><SvgIcons.ArrowDownIcon /></div>
                    }
                    <div onClick={this.onLiClick.bind(this)}>
                        <span className="term-name" dangerouslySetInnerHTML={{ __html: props.term.Name }}></span>
                        {props.isEditable && util.canEdit() && <div className="edit-button" onClick={this.onClick.bind(this)}><SvgIcons.EditIcon /></div>}
                    </div>
                </div>
                <Collapsible isOpened={state.isOpened}>{props.children}</Collapsible>
            </li>
        );
    }
}

Term.propTypes = {
    term: PropTypes.object,
    children: PropTypes.node,
    isEditable: PropTypes.bool,
    onClick: PropTypes.func,
    closeAll: PropTypes.bool
};

export default Term;