import React, {PropTypes} from "react";
import styles from "./style.less";
import { ArrowBack } from "dnn-svg-icons";

/* eslint-disable react/no-danger */
const BackToMain = ({onClick, label}) => {
    return (
        <div         
            className={styles.backTo} 
            onClick={onClick}>
            <div className="icon" dangerouslySetInnerHTML={{ __html: ArrowBack }} />
            <span>{label}</span>
        </div>
    );
};

BackToMain.propTypes = {
    label: PropTypes.string.isRequired,
    onClick: PropTypes.func
};

export default BackToMain;