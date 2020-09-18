import React, { Component } from "react";
import PropTypes from "prop-types";
import { GridCell, SvgIcons } from "@dnnsoftware/dnn-react-common";
import styles from "./style.less";
import ColumnSizes from "../ExtensionColumnSizes";
import InUseModal from "../InUseModal";

class ExtensionDetailRow extends Component {
    constructor() {
        super();
        this.state = {
            inUseModalOpen: false,
            usagePackageName: null,
            usagePackageId: -1
        };
    }

    toggleInUseModal(packageName, packageId) {
        let nextState = !this.state.inUseModalOpen;
        window.dnn.stopEscapeFromClosingPB = nextState;
        this.setState({
            inUseModalOpen: nextState,
            usagePackageName: packageName,
            usagePackageId: packageId
        });
    }

    getInUseDisplay(packageName, packageId) {
        const {props} = this;
        if (props._package.inUse === "Yes") {
            return <div className="in-use" onClick={this.toggleInUseModal.bind(this, packageName, packageId)}>{props._package.inUse}</div>;
        }
        else {
            return <p>{props._package.inUse}</p>;
        }
    }

    /* eslint-disable react/no-danger */
    render() {
        const {props, state} = this;
        return (
            <div>
                <GridCell className={styles.extensionDetailRow} columnSize={100} style={{ padding: "20px 0 20px 20px" }}>
                    <GridCell columnSize={ColumnSizes[0]} style={{ padding: 0 }}>
                        <img src={props._package.packageIcon && props._package.packageIcon.replace("~", "")} alt={props._package.friendlyName} />
                    </GridCell>
                    <GridCell columnSize={ColumnSizes[1]} style={{ padding: "0 35px" }}>
                        <span className="package-name">{props._package.friendlyName}</span>
                        <p dangerouslySetInnerHTML={{ __html: props._package.description }}></p>
                    </GridCell>
                    <GridCell columnSize={ColumnSizes[2]}>
                        <p>{props._package.version}</p>
                    </GridCell>
                    <GridCell columnSize={ColumnSizes[3]}>
                        {this.getInUseDisplay(props._package.friendlyName, props._package.packageId)}
                    </GridCell>
                    <GridCell columnSize={ColumnSizes[4]}>
                        <a href={props._package.url} target="_blank" rel="noopener noreferrer" aria-label="Update">
                            <img src={props._package.upgradeIndicator} alt="Update" />
                        </a>
                    </GridCell>
                    <GridCell columnSize={ColumnSizes[5]} style={{ paddingRight: 0 }}>
                        {(props._package.canDelete && props.isHost) && <div className="extension-action" dangerouslySetInnerHTML={{ __html: SvgIcons.TrashIcon }} onClick={props.onDelete}></div>}
                        <div className="extension-action" onClick={props.onEdit.bind(this, props._package.packageId)} dangerouslySetInnerHTML={{ __html: SvgIcons.EditIcon }}></div>
                    </GridCell>
                </GridCell >
                {state.inUseModalOpen &&
                    <InUseModal
                        isHost={props.isHost}
                        packageId={state.usagePackageId}
                        packageName={state.usagePackageName}
                        fixedHeight={500}
                        isOpened={state.inUseModalOpen}
                        onClose={this.toggleInUseModal.bind(this)}>
                    </InUseModal>
                }
            </div>
        );
    }
}

ExtensionDetailRow.propTypes = {
    _package: PropTypes.object,
    onEdit: PropTypes.func,
    onDelete: PropTypes.func,
    isHost: PropTypes.bool
};

export default ExtensionDetailRow;