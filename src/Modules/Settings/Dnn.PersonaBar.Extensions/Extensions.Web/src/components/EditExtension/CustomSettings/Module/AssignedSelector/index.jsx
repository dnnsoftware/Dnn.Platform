import React, { PropTypes, Component } from "react";
import GridCell from "dnn-grid-cell";
import { Scrollbars } from "react-custom-scrollbars";
import { ArrowLeftIcon, ArrowRightIcon } from "dnn-svg-icons";
import styles from "./style.less";
class AssignedSelector extends Component {
    constructor() {
        super();
        this.state = {
            assignedPortals: [],
            unassignedPortals: []
        };
    }
    componentWillMount() {
        const { props } = this;
        this.setState({
            assignedPortals: props.assignedPortals,
            unassignedPortals: props.unassignedPortals
        });
    }
    onClickOnPortal(portal) {
        portal.selected = !portal.selected;

        this.setState({});
    }
    getPortalList(list) {
        const { state } = this;
        return list.map((portal) => {
            return <li className={portal.selected ? "selected" : ""}
                onClick={this.onClickOnPortal.bind(this, portal)}>
                {portal.Name}
            </li>;
        });
    }
    moveItemsRight() {
        let { assignedPortals } = this.state;
        let itemsToStay = [], itemsToMove = [];
        assignedPortals.forEach((portal) => {
            let {selected} = portal;
            delete portal.selected;
            selected ? itemsToMove.push(portal) : itemsToStay.push(portal);
        });

        this.setState({
            assignedPortals: itemsToStay,
            unassignedPortals: this.state.unassignedPortals.concat(itemsToMove)
        });

    }
    moveItemsLeft() {
        let { unassignedPortals } = this.state;
        let itemsToStay = [], itemsToMove = [];
        unassignedPortals.forEach((portal) => {
            let {selected} = portal;
            delete portal.selected;
            selected ? itemsToMove.push(portal) : itemsToStay.push(portal);
        });

        this.setState({
            unassignedPortals: itemsToStay,
            assignedPortals: this.state.assignedPortals.concat(itemsToMove)
        });
    }
    moveAll(direction){
        switch(direction){
            case "right":
                this.setState({
                    unassignedPortals: this.state.unassignedPortals.concat(this.state.assignedPortals),
                    assignedPortals: []
                });
                break;
            default:
                this.setState({
                    assignedPortals: this.state.assignedPortals.concat(this.state.unassignedPortals),
                    unassignedPortals: []
                });
                break;
        }
    }
    /* eslint-disable react/no-danger */
    render() {
        const {props, state} = this;
        const assignedPortals = this.getPortalList(state.assignedPortals);
        const unassignedPortals = this.getPortalList(state.unassignedPortals);
        return (
            <GridCell className={styles.assignedSelector}>
                <GridCell columnSize={45} className="selector-box">
                    <Scrollbars style={{ width: "100%", height: "100%" }}>
                        <ul>
                            {assignedPortals}
                        </ul>
                    </Scrollbars>
                </GridCell>
                <GridCell columnSize={10} className="selector-controls">
                    <div href="" className="move-item single-right" onClick={this.moveItemsRight.bind(this)} dangerouslySetInnerHTML={{ __html: ArrowRightIcon }}></div>
                    <div href="" className="move-item single-left" onClick={this.moveItemsLeft.bind(this)} dangerouslySetInnerHTML={{ __html: ArrowLeftIcon }}></div>
                    <div href="" className="move-item double-right" onClick={this.moveAll.bind(this, "right")} dangerouslySetInnerHTML={{ __html: ArrowRightIcon }}></div>
                    <div href="" className="move-item double-left" onClick={this.moveAll.bind(this)} dangerouslySetInnerHTML={{ __html: ArrowLeftIcon }}></div>
                </GridCell>
                <GridCell columnSize={45} className="selector-box">
                    <Scrollbars style={{ width: "100%", height: "100%" }}>
                        <ul>
                            {unassignedPortals}
                        </ul>
                    </Scrollbars>
                </GridCell>
            </GridCell>
        );
        // <p className="modal-pagination"> --1 of 2 -- </p>
    }
}

AssignedSelector.PropTypes = {
    assignedPortals: PropTypes.array,
    unassignedPortals: PropTypes.array
};

export default AssignedSelector;