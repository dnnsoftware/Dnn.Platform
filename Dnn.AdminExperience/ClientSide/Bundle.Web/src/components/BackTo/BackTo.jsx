import PropTypes from "prop-types";
import React from "react";
import styles from "./style.module.less";
import { ArrowBack } from "@dnnsoftware/dnn-react-common";

const BackToMain = ({ onClick, label }) => {
  return (
    <div className={styles.backTo} onClick={onClick}>
      <div className="icon">
        <ArrowBack />
      </div>
      <span>{label}</span>
    </div>
  );
};

BackToMain.propTypes = {
  label: PropTypes.string.isRequired,
  onClick: PropTypes.func,
};

export default BackToMain;
