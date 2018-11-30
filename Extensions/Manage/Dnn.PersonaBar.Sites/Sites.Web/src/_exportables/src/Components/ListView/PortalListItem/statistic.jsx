import PropTypes from 'prop-types';
import React from "react";

/* eslint-disable react/no-danger */
const Statistic = ({label, value}) => (
    <div className="portal-statistic">
        <div className="statistic-label">
            <p>{label}: <span>{value}</span></p>
        </div>
    </div>
);

Statistic.propTypes = {
    label: PropTypes.string,
    value: PropTypes.oneOfType([PropTypes.string, PropTypes.number])
};
export default Statistic;